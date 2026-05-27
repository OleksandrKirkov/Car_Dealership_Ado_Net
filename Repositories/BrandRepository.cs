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
            INSERT INTO brands (name, country)
            VALUES (@name, @country);

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", request.Name);
        command.Parameters.AddWithValue("@country", request.Country);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<BrandResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT brand_id, name, country
            FROM brands
            ORDER BY brand_id;
            """;

        List<BrandResponse> brands = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            brands.Add(new BrandResponse(
                BrandId: reader.GetInt32("brand_id"),
                Name: reader.GetString("name"),
                Country: reader.GetString("country")
            ));
        }

        return brands;
    }

    public async Task<BrandResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT brand_id, name, country
            FROM brands
            WHERE brand_id = @id;
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

        return new BrandResponse(
            BrandId: reader.GetInt32("brand_id"),
            Name: reader.GetString("name"),
            Country: reader.GetString("country")
        );
    }

    public async Task<bool> UpdateAsync(int id, UpdateBrandRequest request)
    {
        const string sql = """
            UPDATE brands
            SET name = @name,
                country = @country
            WHERE brand_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@name", request.Name);
        command.Parameters.AddWithValue("@country", request.Country);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM brands
            WHERE brand_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }
}
