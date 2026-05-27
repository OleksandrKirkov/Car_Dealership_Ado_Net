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
            INSERT INTO Cars (
                Model_ID,
                Supplier_ID,
                Vin,
                Manufacture_Year,
                Color,
                Mileage,
                Price,
                Status
            )
            VALUES (
                @ModelId,
                @SupplierId,
                @Vin,
                @ManufactureYear,
                @Color,
                @Mileage,
                @Price,
                @Status
            );

            SELECT LAST_INSERT_ID();
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ModelId", request.ModelId);
        command.Parameters.AddWithValue("@SupplierId", request.SupplierId);
        command.Parameters.AddWithValue("@Vin", request.Vin);
        command.Parameters.AddWithValue("@ManufactureYear", request.ManufactureYear);
        command.Parameters.AddWithValue("@Color", request.Color);
        command.Parameters.AddWithValue("@Mileage", request.Mileage);
        command.Parameters.AddWithValue("@Price", request.Price);
        command.Parameters.AddWithValue("@Status", request.Status);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<IReadOnlyList<CarViewResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT
                Cars.ID,
                Brands.Name AS Brand,
                Car_Models.Name AS Model,
                Suppliers.Name AS Supplier,
                Cars.Vin,
                Cars.Manufacture_Year,
                Cars.Color,
                Cars.Mileage,
                Cars.Price,
                Cars.Status
            FROM Cars
            JOIN Car_Models ON Cars.Model_ID = Car_Models.ID
            JOIN Brands ON Car_Models.Brand_ID = Brands.ID
            JOIN Suppliers ON Cars.Supplier_ID = Suppliers.ID
            ORDER BY Cars.ID;
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
                Cars.ID,
                Brands.Name AS Brand,
                Car_Models.Name AS Model,
                Suppliers.Name AS Supplier,
                Cars.Vin,
                Cars.Manufacture_Year,
                Cars.Color,
                Cars.Mileage,
                Cars.Price,
                Cars.Status
            FROM Cars
            JOIN Car_Models ON Cars.Model_ID = Car_Models.ID
            JOIN Brands ON Car_Models.Brand_ID = Brands.ID
            JOIN Suppliers ON Cars.Supplier_ID = Suppliers.ID
            WHERE Cars.ID = @ID;
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

        return ReadCarView(reader);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCarRequest request)
    {
        const string sql = """
            UPDATE Cars
            SET Model_ID = @ModelId,
                Supplier_ID = @SupplierId,
                Vin = @Vin,
                Manufacture_Year = @ManufactureYear,
                Color = @Color,
                Mileage = @Mileage,
                Price = @Price,
                Status = @Status
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@ModelID", request.ModelId);
        command.Parameters.AddWithValue("@SupplierID", request.SupplierId);
        command.Parameters.AddWithValue("@Vin", request.Vin);
        command.Parameters.AddWithValue("@ManufactureYear", request.ManufactureYear);
        command.Parameters.AddWithValue("@Color", request.Color);
        command.Parameters.AddWithValue("@Mileage", request.Mileage);
        command.Parameters.AddWithValue("@Price", request.Price);
        command.Parameters.AddWithValue("@Status", request.Status);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = """
            DELETE FROM Cars
            WHERE ID = @ID;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", id);

        int affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    private static CarViewResponse ReadCarView(MySqlDataReader reader)
    {
        return new CarViewResponse(
            CarId: reader.GetInt32("ID"),
            Brand: reader.GetString("Brand"),
            Model: reader.GetString("Model"),
            Supplier: reader.GetString("Supplier"),
            Vin: reader.GetString("Vin"),
            ManufactureYear: reader.GetInt32("Manufacture_Year"),
            Color: reader.GetString("Color"),
            Mileage: reader.GetInt32("Mileage"),
            Price: reader.GetDecimal("Price"),
            Status: reader.GetString("Status")
        );
    }
}
