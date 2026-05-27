using CarDealershipAdoNet.Database;
using CarDealershipAdoNet.DTOs;
using MySqlConnector;

namespace CarDealershipAdoNet.Repositories;

public interface IClientRepository
{
    Task<int> CreateAsync(CreateClientRequest request);
    Task<IReadOnlyList<ClientResponse>> GetAllAsync();
    Task<ClientResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateClientRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class ClientRepository : IClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(CreateClientRequest request)
    {
        const string sql = """
            INSERT INTO clients (
                first_name,
                last_name,
                phone,
                email,
                passport_number
            )
            VALUES (
                @firstName,
                @lastName,
                @phone,
                @email,
                @passportNumber
            );

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@firstName", request.FirstName);
        command.Parameters.AddWithValue("@lastName", request.LastName);
        command.Parameters.AddWithValue("@phone", request.Phone);
        command.Parameters.AddWithValue("@email", request.Email);
        command.Parameters.AddWithValue("@passportNumber", request.PassportNumber);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT
                client_id,
                first_name,
                last_name,
                phone,
                email,
                passport_number
            FROM clients
            ORDER BY client_id;
            """;

        List<ClientResponse> clients = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            clients.Add(new ClientResponse(
                ClientId: reader.GetInt32("client_id"),
                FirstName: reader.GetString("first_name"),
                LastName: reader.GetString("last_name"),
                Phone: reader.GetString("phone"),
                Email: reader.GetString("email"),
                PassportNumber: reader.GetString("passport_number")
            ));
        }

        return clients;
    }

    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT
                client_id,
                first_name,
                last_name,
                phone,
                email,
                passport_number
            FROM clients
            WHERE client_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ClientResponse(
            ClientId: reader.GetInt32("client_id"),
            FirstName: reader.GetString("first_name"),
            LastName: reader.GetString("last_name"),
            Phone: reader.GetString("phone"),
            Email: reader.GetString("email"),
            PassportNumber: reader.GetString("passport_number")
        );
    }

    public async Task<bool> UpdateAsync(int id, UpdateClientRequest request)
    {
        const string sql = """
            UPDATE clients
            SET first_name = @firstName,
                last_name = @lastName,
                phone = @phone,
                email = @email,
                passport_number = @passportNumber
            WHERE client_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@firstName", request.FirstName);
        command.Parameters.AddWithValue("@lastName", request.LastName);
        command.Parameters.AddWithValue("@phone", request.Phone);
        command.Parameters.AddWithValue("@email", request.Email);
        command.Parameters.AddWithValue("@passportNumber", request.PassportNumber);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM clients
            WHERE client_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }
}
