using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Services;
using MySqlConnector;

namespace CarDealershipAdoNet.Endpoints;

public static class CarEndpoints
{
    public static void MapCarEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/cars")
            .WithTags("Cars CRUD");

        group.MapPost("/", async (ICarService carService, CreateCarRequest request) =>
        {
            try
            {
                int id = await carService.CreateAsync(request);

                return Results.Created($"/api/cars/{id}", new
                {
                    carId = id,
                    request.ModelId,
                    request.SupplierId,
                    request.Vin,
                    request.ManufactureYear,
                    request.Color,
                    request.Mileage,
                    request.Price,
                    request.Status
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Car was not created",
                    sqlError = ex.Message
                });
            }
        });

        group.MapGet("/", async (ICarService carService) =>
        {
            IReadOnlyList<CarViewResponse> cars = await carService.GetAllAsync();

            return Results.Ok(cars);
        });

        group.MapGet("/{id:int}", async (ICarService carService, int id) =>
        {
            CarViewResponse? car = await carService.GetByIdAsync(id);

            if (car is null)
            {
                return Results.NotFound(new
                {
                    message = "Car not found"
                });
            }

            return Results.Ok(car);
        });

        group.MapPut("/{id:int}", async (
            ICarService carService,
            int id,
            UpdateCarRequest request
        ) =>
        {
            try
            {
                bool updated = await carService.UpdateAsync(id, request);

                if (!updated)
                {
                    return Results.NotFound(new
                    {
                        message = "Car not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Car updated"
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Car was not updated",
                    sqlError = ex.Message
                });
            }
        });

        group.MapDelete("/{id:int}", async (ICarService carService, int id) =>
        {
            try
            {
                bool deleted = await carService.DeleteAsync(id);

                if (!deleted)
                {
                    return Results.NotFound(new
                    {
                        message = "Car not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Car deleted"
                });
            }
            catch (MySqlException ex)
            {
                return Results.Conflict(new
                {
                    message = "Car cannot be deleted because it is used in sales or test drives",
                    sqlError = ex.Message
                });
            }
        });
    }
}
