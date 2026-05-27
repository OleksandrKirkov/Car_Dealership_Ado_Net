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
                Sales.ID,
                Sales.Sale_Date,
                CONCAT(Clients.First_Name, ' ', Clients.Last_Name) AS Client,
                CONCAT(Employees.First_name, ' ', Employees.Last_Name) AS Employee,
                Brands.Name AS Brand,
                Car_Models.Name AS Model,
                Cars.Vin,
                Sales.Final_Price,
                Sales.Payment_Status
            FROM Sales
            JOIN Clients ON Sales.Client_ID = Clients.ID
            JOIN Employees ON Sales.Employee_ID = Employees.ID
            JOIN Cars ON Sales.Car_ID = Cars.ID
            JOIN Car_Models ON Cars.Model_ID = Car_Models.ID
            JOIN Brands ON Car_Models.Brand_ID = Brands.ID
            ORDER BY Sales.Sale_Date DESC;
            """;

        List<SaleJoinResponse> sales = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sales.Add(new SaleJoinResponse(
                SaleId: reader.GetInt32("ID"),
                SaleDate: reader.GetDateTime("Sale_Date"),
                Client: reader.GetString("Client"),
                Employee: reader.GetString("Employee"),
                Brand: reader.GetString("Brand"),
                Model: reader.GetString("Model"),
                Vin: reader.GetString("Vin"),
                FinalPrice: reader.GetDecimal("Final_Price"),
                PaymentStatus: reader.GetString("Payment_Status")
            ));
        }

        return sales;
    }

    public async Task<IReadOnlyList<CarViewResponse>> GetAvailableCarsAsync(decimal maxPrice)
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
            WHERE Cars.Status = 'available'
              AND Cars.Price <= @maxPrice
            ORDER BY Cars.Price ASC;
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
                Brands.Name AS Brand,
                COUNT(Cars.ID) AS Cars_Count,
                MIN(Cars.Price) AS Min_Price,
                MAX(Cars.Price) AS Max_Price,
                ROUND(AVG(Cars.Price), 2) AS Average_Price
            FROM Brands
            JOIN Car_Models ON Brands.ID = Car_Models.Brand_ID
            JOIN Cars ON Car_Models.ID = Cars.Model_ID
            GROUP BY Brands.ID, Brands.Name
            ORDER BY Cars_Count DESC;
            """;

        List<CarsCountByBrandResponse> rows = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rows.Add(new CarsCountByBrandResponse(
                Brand: reader.GetString("Brand"),
                CarsCount: reader.GetInt32("Cars_Count"),
                MinPrice: reader.GetDecimal("Min_Price"),
                MaxPrice: reader.GetDecimal("Max_Price"),
                AveragePrice: reader.GetDecimal("Average_Price")
            ));
        }

        return rows;
    }

    public async Task<TotalSalesResponse> GetTotalSalesAsync()
    {
        const string sql = """
            SELECT
                COUNT(*) AS Sales_Count,
                COALESCE(SUM(Final_Price), 0) AS Total_Sales_Amount,
                COALESCE(AVG(Final_Price), 0) AS Average_Sale_Amount,
                COALESCE(MIN(Final_Price), 0) AS Min_Sale_Amount,
                COALESCE(MAX(Final_Price), 0) AS Max_Sale_Amount
            FROM Sales;
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
            SalesCount: reader.GetInt32("Sales_Count"),
            TotalSalesAmount: reader.GetDecimal("Total_Sales_Amount"),
            AverageSaleAmount: reader.GetDecimal("Average_Sale_Amount"),
            MinSaleAmount: reader.GetDecimal("Min_Sale_Amount"),
            MaxSaleAmount: reader.GetDecimal("Max_Sale_Amount")
        );
    }

    public async Task<IReadOnlyList<PaymentBySaleResponse>> GetPaymentsBySaleAsync()
    {
        const string sql = """
            SELECT
                Sales.ID,
                CONCAT(Clients.First_Name, ' ', Clients.Last_Name) AS Client,
                Sales.Final_Price,
                COALESCE(SUM(Payments.Amount), 0) AS Paid_Amount,
                Sales.Final_Price - COALESCE(SUM(Payments.Amount), 0) AS Remaining_Amount,
                Sales.Payment_Status
            FROM Sales
            JOIN Clients ON Sales.Client_ID = Clients.ID
            LEFT JOIN Payments ON Sales.ID = Payments.Sale_ID
            GROUP BY
                Sales.ID,
                Clients.First_Name,
                Clients.Last_Name,
                Sales.Final_Price,
                Sales.Payment_Status
            ORDER BY Sales.ID;
            """;

        List<PaymentBySaleResponse> rows = new();

        await using MySqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using MySqlCommand command = new MySqlCommand(sql, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rows.Add(new PaymentBySaleResponse(
                SaleId: reader.GetInt32("ID"),
                Client: reader.GetString("Client"),
                FinalPrice: reader.GetDecimal("Final_Price"),
                PaidAmount: reader.GetDecimal("Paid_Amount"),
                RemainingAmount: reader.GetDecimal("Remaining_Amount"),
                PaymentStatus: reader.GetString("Payment_Status")
            ));
        }

        return rows;
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
