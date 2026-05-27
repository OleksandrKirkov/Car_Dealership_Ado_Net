namespace CarDealershipAdoNet.DTOs;

public sealed record CreateClientRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string PassportNumber
);

public sealed record UpdateClientRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string PassportNumber
);

public sealed record ClientResponse(
    int ClientId,
    string FirstName,
    string LastName,
    string Phone,
    string Email,
    string PassportNumber
);
