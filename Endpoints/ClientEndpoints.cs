using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Services;
using MySqlConnector;

namespace CarDealershipAdoNet.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/clients")
            .WithTags("Clients CRUD");

        group.MapPost("/", async (IClientService clientService, CreateClientRequest request) =>
        {
            try
            {
                int id = await clientService.CreateAsync(request);

                return Results.Created($"/api/clients/{id}", new
                {
                    clientId = id,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    request.Email,
                    request.PassportNumber
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Client was not created",
                    sqlError = ex.Message
                });
            }
        });

        group.MapGet("/", async (IClientService clientService) =>
        {
            IReadOnlyList<ClientResponse> clients = await clientService.GetAllAsync();

            return Results.Ok(clients);
        });

        group.MapGet("/{id:int}", async (IClientService clientService, int id) =>
        {
            ClientResponse? client = await clientService.GetByIdAsync(id);

            if (client is null)
            {
                return Results.NotFound(new
                {
                    message = "Client not found"
                });
            }

            return Results.Ok(client);
        });

        group.MapPut("/{id:int}", async (
            IClientService clientService,
            int id,
            UpdateClientRequest request
        ) =>
        {
            try
            {
                bool updated = await clientService.UpdateAsync(id, request);

                if (!updated)
                {
                    return Results.NotFound(new
                    {
                        message = "Client not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Client updated"
                });
            }
            catch (MySqlException ex)
            {
                return Results.BadRequest(new
                {
                    message = "Client was not updated",
                    sqlError = ex.Message
                });
            }
        });

        group.MapDelete("/{id:int}", async (IClientService clientService, int id) =>
        {
            try
            {
                bool deleted = await clientService.DeleteAsync(id);

                if (!deleted)
                {
                    return Results.NotFound(new
                    {
                        message = "Client not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Client deleted"
                });
            }
            catch (MySqlException ex)
            {
                return Results.Conflict(new
                {
                    message = "Client cannot be deleted because it is used in sales or test drives",
                    sqlError = ex.Message
                });
            }
        });
    }
}
