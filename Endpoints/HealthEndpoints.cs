using CarDealershipAdoNet.Database;
using MySqlConnector;

namespace CarDealershipAdoNet.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", async (IDbConnectionFactory connectionFactory) =>
        {
            await using MySqlConnection connection = connectionFactory.CreateConnection();

            try
            {
                await connection.OpenAsync();

                return Results.Ok(new
                {
                    status = "OK",
                    database = "Connected"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Database connection failed",
                    detail: ex.Message
                );
            }
        })
        .WithTags("Health");
    }
}
