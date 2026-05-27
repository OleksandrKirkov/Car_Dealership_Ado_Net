using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Services;
using MySqlConnector;

namespace CarDealershipAdoNet.Endpoints;

public static class BrandEndpoints
{
    public static void MapBrandEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/brands")
            .WithTags("Brands CRUD");

        group.MapPost("/", async (IBrandService brandService, CreateBrandRequest request) =>
        {
            try
            {
                int id = await brandService.CreateAsync(request);

                return Results.Created($"/api/brands/{id}", new
                {
                    brandId = id,
                    request.Name,
                    request.Country
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Brand was not created",
                    sqlError = ex.Message
                });
            }
        });

        group.MapGet("/", async (IBrandService brandService) =>
        {
            IReadOnlyList<BrandResponse> brands = await brandService.GetAllAsync();

            return Results.Ok(brands);
        });

        group.MapGet("/{id:int}", async (IBrandService brandService, int id) =>
        {
            BrandResponse? brand = await brandService.GetByIdAsync(id);

            if (brand is null)
            {
                return Results.NotFound(new
                {
                    message = "Brand not found"
                });
            }

            return Results.Ok(brand);
        });

        group.MapPut("/{id:int}", async (
            IBrandService brandService,
            int id,
            UpdateBrandRequest request
        ) =>
        {
            try
            {
                bool updated = await brandService.UpdateAsync(id, request);

                if (!updated)
                {
                    return Results.NotFound(new
                    {
                        message = "Brand not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Brand updated"
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Brand was not updated",
                    sqlError = ex.Message
                });
            }
        });

        group.MapDelete("/{id:int}", async (IBrandService brandService, int id) =>
        {
            try
            {
                bool deleted = await brandService.DeleteAsync(id);

                if (!deleted)
                {
                    return Results.NotFound(new
                    {
                        message = "Brand not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Brand deleted"
                });
            }
            catch (MySqlException ex)
            {
                return Results.Conflict(new
                {
                    message = "Brand cannot be deleted because it is used by car models",
                    sqlError = ex.Message
                });
            }
        });
    }
}
