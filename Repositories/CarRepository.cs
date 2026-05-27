using CarDealershipAdoNet.Database;
using CarDealershipAdoNet.DTOs;
using MySqlConnector;

namespace CarDealershipAdoNet.Repositories;

public interface ICarRepository
{
    Task<int> CreateAsync(CreateCarRequest request);
    Task<IReadOnlyList<CarViewResponse>> GetAllAsync();
    Task<CarViewResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateCarRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class CarRepository : ICarRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CarRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(CreateCarRequest request)
    {
        const string sql = """
            INSERT INTO cars (
                model_id,
                supplier_id,
                vin,
                manufacture_year,
                color,
                mileage,
                price,
                status
            )
            VALUES (
                @modelId,
                @supplierId,
                @vin,
                @manufactureYear,
                @color,
                @mileage,
                @price,
                @status
            );

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@modelId", request.ModelId);
        command.Parameters.AddWithValue("@supplierId", request.SupplierId);
        command.Parameters.AddWithValue("@vin", request.Vin);
        command.Parameters.AddWithValue("@manufactureYear", request.ManufactureYear);
        command.Parameters.AddWithValue("@color", request.Color);
        command.Parameters.AddWithValue("@mileage", request.Mileage);
        command.Parameters.AddWithValue("@price", request.Price);
        command.Parameters.AddWithValue("@status", request.Status);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<CarViewResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT
                cars.car_id,
                brands.name AS brand,
                car_models.name AS model,
                suppliers.name AS supplier,
                cars.vin,
                cars.manufacture_year,
                cars.color,
                cars.mileage,
                cars.price,
                cars.status
            FROM cars
            JOIN car_models ON cars.model_id = car_models.model_id
            JOIN brands ON car_models.brand_id = brands.brand_id
            JOIN suppliers ON cars.supplier_id = suppliers.supplier_id
            ORDER BY cars.car_id;
            """;

        List<CarViewResponse> cars = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            cars.Add(ReadCarView(reader));
        }

        return cars;
    }

    public async Task<CarViewResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT
                cars.car_id,
                brands.name AS brand,
                car_models.name AS model,
                suppliers.name AS supplier,
                cars.vin,
                cars.manufacture_year,
                cars.color,
                cars.mileage,
                cars.price,
                cars.status
            FROM cars
            JOIN car_models ON cars.model_id = car_models.model_id
            JOIN brands ON car_models.brand_id = brands.brand_id
            JOIN suppliers ON cars.supplier_id = suppliers.supplier_id
            WHERE cars.car_id = @id;
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

        return ReadCarView(reader);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCarRequest request)
    {
        const string sql = """
            UPDATE cars
            SET model_id = @modelId,
                supplier_id = @supplierId,
                vin = @vin,
                manufacture_year = @manufactureYear,
                color = @color,
                mileage = @mileage,
                price = @price,
                status = @status
            WHERE car_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@modelId", request.ModelId);
        command.Parameters.AddWithValue("@supplierId", request.SupplierId);
        command.Parameters.AddWithValue("@vin", request.Vin);
        command.Parameters.AddWithValue("@manufactureYear", request.ManufactureYear);
        command.Parameters.AddWithValue("@color", request.Color);
        command.Parameters.AddWithValue("@mileage", request.Mileage);
        command.Parameters.AddWithValue("@price", request.Price);
        command.Parameters.AddWithValue("@status", request.Status);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM cars
            WHERE car_id = @id;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    private static CarViewResponse ReadCarView(MySqlDataReader reader)
    {
        return new CarViewResponse(
            CarId: reader.GetInt32("car_id"),
            Brand: reader.GetString("brand"),
            Model: reader.GetString("model"),
            Supplier: reader.GetString("supplier"),
            Vin: reader.GetString("vin"),
            ManufactureYear: reader.GetInt32("manufacture_year"),
            Color: reader.GetString("color"),
            Mileage: reader.GetInt32("mileage"),
            Price: reader.GetDecimal("price"),
            Status: reader.GetString("status")
        );
    }
}
