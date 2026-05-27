using CarDealershipAdoNet.Database;
using CarDealershipAdoNet.DTOs;
using MySqlConnector;

namespace CarDealershipAdoNet.Repositories;

public interface IBrandRepository
{
    Task<int> CreateAsync(CreateBrandRequest request);
    Task<IReadOnlyList<BrandResponse>> GetAllAsync();
    Task<BrandResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateBrandRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class BrandRepository : IBrandRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BrandRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(CreateBrandRequest request)
    {
        const string sql = """
            INSERT INTO Brands (Name, Country)
            VALUES (@Name, @Country);

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Country", request.Country);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<BrandResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT ID, Name, Country
            FROM Brands
            ORDER BY ID;
            """;

        List<BrandResponse> brands = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            brands.Add(new BrandResponse(
                BrandId: reader.GetInt32("ID"),
                Name: reader.GetString("Name"),
                Country: reader.GetString("Country")
            ));
        }

        return brands;
    }

    public async Task<BrandResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT ID, Name, Country
            FROM Brands
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

        return new BrandResponse(
            BrandId: reader.GetInt32("ID"),
            Name: reader.GetString("Name"),
            Country: reader.GetString("Country")
        );
    }

    public async Task<bool> UpdateAsync(int id, UpdateBrandRequest request)
    {
        const string sql = """
            UPDATE Brands
            SET Name = @Name,
                Country = @Country
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Country", request.Country);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM Brands
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
