namespace CarDealershipAdoNet.DTOs;

public sealed record CreateCarRequest(
    int ModelId,
    int SupplierId,
    string Vin,
    int ManufactureYear,
    string Color,
    int Mileage,
    decimal Price,
    string Status
);

public sealed record UpdateCarRequest(
    int ModelId,
    int SupplierId,
    string Vin,
    int ManufactureYear,
    string Color,
    int Mileage,
    decimal Price,
    string Status
);

public sealed record CarViewResponse(
    int CarId,
    string Brand,
    string Model,
    string Supplier,
    string Vin,
    int ManufactureYear,
    string Color,
    int Mileage,
    decimal Price,
    string Status
);
