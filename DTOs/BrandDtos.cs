namespace CarDealershipAdoNet.DTOs;

public sealed record CreateBrandRequest(
    string Name,
    string Country
);

public sealed record UpdateBrandRequest(
    string Name,
    string Country
);

public sealed record BrandResponse(
    int BrandId,
    string Name,
    string Country
);
