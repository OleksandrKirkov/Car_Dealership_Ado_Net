using CarDealershipAdoNet.Database;
using CarDealershipAdoNet.DTOs;
using MySqlConnector;

namespace CarDealershipAdoNet.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<SaleJoinResponse>> GetSalesJoinAsync();
    Task<IReadOnlyList<CarViewResponse>> GetAvailableCarsAsync(decimal maxPrice);
    Task<IReadOnlyList<CarsCountByBrandResponse>> GetCarsCountByBrandAsync();
    Task<TotalSalesResponse> GetTotalSalesAsync();
    Task<IReadOnlyList<PaymentBySaleResponse>> GetPaymentsBySaleAsync();
}

public sealed class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReportRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<SaleJoinResponse>> GetSalesJoinAsync()
    {
        const string sql = """
            SELECT
                sales.sale_id,
                sales.sale_date,
                CONCAT(clients.first_name, ' ', clients.last_name) AS client,
                CONCAT(employees.first_name, ' ', employees.last_name) AS employee,
                brands.name AS brand,
                car_models.name AS model,
                cars.vin,
                sales.final_price,
                sales.payment_status
            FROM sales
            JOIN clients ON sales.client_id = clients.client_id
            JOIN employees ON sales.employee_id = employees.employee_id
            JOIN cars ON sales.car_id = cars.car_id
            JOIN car_models ON cars.model_id = car_models.model_id
            JOIN brands ON car_models.brand_id = brands.brand_id
            ORDER BY sales.sale_date DESC;
            """;

        List<SaleJoinResponse> sales = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sales.Add(new SaleJoinResponse(
                SaleId: reader.GetInt32("sale_id"),
                SaleDate: reader.GetDateTime("sale_date"),
                Client: reader.GetString("client"),
                Employee: reader.GetString("employee"),
                Brand: reader.GetString("brand"),
                Model: reader.GetString("model"),
                Vin: reader.GetString("vin"),
                FinalPrice: reader.GetDecimal("final_price"),
                PaymentStatus: reader.GetString("payment_status")
            ));
        }

        return sales;
    }

    public async Task<IReadOnlyList<CarViewResponse>> GetAvailableCarsAsync(decimal maxPrice)
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
            WHERE cars.status = 'available'
              AND cars.price <= @maxPrice
            ORDER BY cars.price ASC;
            """;

        List<CarViewResponse> cars = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@maxPrice", maxPrice);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            cars.Add(ReadCarView(reader));
        }

        return cars;
    }

    public async Task<IReadOnlyList<CarsCountByBrandResponse>> GetCarsCountByBrandAsync()
    {
        const string sql = """
            SELECT
                brands.name AS brand,
                COUNT(cars.car_id) AS cars_count,
                MIN(cars.price) AS min_price,
                MAX(cars.price) AS max_price,
                ROUND(AVG(cars.price), 2) AS average_price
            FROM brands
            JOIN car_models ON brands.brand_id = car_models.brand_id
            JOIN cars ON car_models.model_id = cars.model_id
            GROUP BY brands.brand_id, brands.name
            ORDER BY cars_count DESC;
            """;

        List<CarsCountByBrandResponse> rows = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rows.Add(new CarsCountByBrandResponse(
                Brand: reader.GetString("brand"),
                CarsCount: reader.GetInt32("cars_count"),
                MinPrice: reader.GetDecimal("min_price"),
                MaxPrice: reader.GetDecimal("max_price"),
                AveragePrice: reader.GetDecimal("average_price")
            ));
        }

        return rows;
    }

    public async Task<TotalSalesResponse> GetTotalSalesAsync()
    {
        const string sql = """
            SELECT
                COUNT(*) AS sales_count,
                COALESCE(SUM(final_price), 0) AS total_sales_amount,
                COALESCE(AVG(final_price), 0) AS average_sale_amount,
                COALESCE(MIN(final_price), 0) AS min_sale_amount,
                COALESCE(MAX(final_price), 0) AS max_sale_amount
            FROM sales;
            """;

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return new TotalSalesResponse(
                SalesCount: 0,
                TotalSalesAmount: 0,
                AverageSaleAmount: 0,
                MinSaleAmount: 0,
                MaxSaleAmount: 0
            );
        }

        return new TotalSalesResponse(
            SalesCount: reader.GetInt32("sales_count"),
            TotalSalesAmount: reader.GetDecimal("total_sales_amount"),
            AverageSaleAmount: reader.GetDecimal("average_sale_amount"),
            MinSaleAmount: reader.GetDecimal("min_sale_amount"),
            MaxSaleAmount: reader.GetDecimal("max_sale_amount")
        );
    }

    public async Task<IReadOnlyList<PaymentBySaleResponse>> GetPaymentsBySaleAsync()
    {
        const string sql = """
            SELECT
                sales.sale_id,
                CONCAT(clients.first_name, ' ', clients.last_name) AS client,
                sales.final_price,
                COALESCE(SUM(payments.amount), 0) AS paid_amount,
                sales.final_price - COALESCE(SUM(payments.amount), 0) AS remaining_amount,
                sales.payment_status
            FROM sales
            JOIN clients ON sales.client_id = clients.client_id
            LEFT JOIN payments ON sales.sale_id = payments.sale_id
            GROUP BY
                sales.sale_id,
                clients.first_name,
                clients.last_name,
                sales.final_price,
                sales.payment_status
            ORDER BY sales.sale_id;
            """;

        List<PaymentBySaleResponse> rows = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rows.Add(new PaymentBySaleResponse(
                SaleId: reader.GetInt32("sale_id"),
                Client: reader.GetString("client"),
                FinalPrice: reader.GetDecimal("final_price"),
                PaidAmount: reader.GetDecimal("paid_amount"),
                RemainingAmount: reader.GetDecimal("remaining_amount"),
                PaymentStatus: reader.GetString("payment_status")
            ));
        }

        return rows;
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
