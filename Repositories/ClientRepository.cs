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
            INSERT INTO Clients (
                First_Name,
                Last_Name,
                Phone,
                Email,
                Passport_Number
            )
            VALUES (
                @FirstName,
                @LastName,
                @Phone,
                @Email,
                @PassportNumber
            );

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", request.FirstName);
        command.Parameters.AddWithValue("@LastName", request.LastName);
        command.Parameters.AddWithValue("@Phone", request.Phone);
        command.Parameters.AddWithValue("@Email", request.Email);
        command.Parameters.AddWithValue("@PassportNumber", request.PassportNumber);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT
                ID,
                First_Name,
                Last_Name,
                Phone,
                Email,
                Passport_Number
            FROM Clients
            ORDER BY ID;
            """;

        List<ClientResponse> clients = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            clients.Add(new ClientResponse(
                ClientId: reader.GetInt32("ID"),
                FirstName: reader.GetString("First_Name"),
                LastName: reader.GetString("Last_Name"),
                Phone: reader.GetString("Phone"),
                Email: reader.GetString("Email"),
                PassportNumber: reader.GetString("Passport_Number")
            ));
        }

        return clients;
    }

    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT
                ID,
                First_Name,
                Last_Name,
                Phone,
                Email,
                Passport_Number
            FROM Clients
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ClientResponse(
            ClientId: reader.GetInt32("ID"),
            FirstName: reader.GetString("First_Name"),
            LastName: reader.GetString("Last_Name"),
            Phone: reader.GetString("Phone"),
            Email: reader.GetString("Email"),
            PassportNumber: reader.GetString("Passport_Number")
        );
    }

    public async Task<bool> UpdateAsync(int id, UpdateClientRequest request)
    {
        const string sql = """
            UPDATE Clients
            SET First_Name = @FirstName,
                Last_Name = @LastName,
                Phone = @phone,
                Email = @Email,
                Passport_Number = @PassportNumber
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@FirstName", request.FirstName);
        command.Parameters.AddWithValue("@LastName", request.LastName);
        command.Parameters.AddWithValue("@Phone", request.Phone);
        command.Parameters.AddWithValue("@Email", request.Email);
        command.Parameters.AddWithValue("@PassportNumber", request.PassportNumber);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM Clients
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }
}
