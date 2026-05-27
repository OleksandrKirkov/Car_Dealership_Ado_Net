namespace CarDealershipAdoNet.DTOs;

public sealed record SaleJoinResponse(
    int SaleId,
    DateTime SaleDate,
    string Client,
    string Employee,
    string Brand,
    string Model,
    string Vin,
    decimal FinalPrice,
    string PaymentStatus
);

public sealed record CarsCountByBrandResponse(
    string Brand,
    int CarsCount,
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePrice
);

public sealed record TotalSalesResponse(
    int SalesCount,
    decimal TotalSalesAmount,
    decimal AverageSaleAmount,
    decimal MinSaleAmount,
    decimal MaxSaleAmount
);

public sealed record PaymentBySaleResponse(
    int SaleId,
    string Client,
    decimal FinalPrice,
    decimal PaidAmount,
    decimal RemainingAmount,
    string PaymentStatus
);
