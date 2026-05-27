using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Repositories;

namespace CarDealershipAdoNet.Services;

public interface IReportService
{
    Task<IReadOnlyList<SaleJoinResponse>> GetSalesJoinAsync();
    Task<IReadOnlyList<CarViewResponse>> GetAvailableCarsAsync(decimal maxPrice);
    Task<IReadOnlyList<CarsCountByBrandResponse>> GetCarsCountByBrandAsync();
    Task<TotalSalesResponse> GetTotalSalesAsync();
    Task<IReadOnlyList<PaymentBySaleResponse>> GetPaymentsBySaleAsync();
}

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<IReadOnlyList<SaleJoinResponse>> GetSalesJoinAsync()
    {
        return _reportRepository.GetSalesJoinAsync();
    }

    public Task<IReadOnlyList<CarViewResponse>> GetAvailableCarsAsync(decimal maxPrice)
    {
        return _reportRepository.GetAvailableCarsAsync(maxPrice);
    }

    public Task<IReadOnlyList<CarsCountByBrandResponse>> GetCarsCountByBrandAsync()
    {
        return _reportRepository.GetCarsCountByBrandAsync();
    }

    public Task<TotalSalesResponse> GetTotalSalesAsync()
    {
        return _reportRepository.GetTotalSalesAsync();
    }

    public Task<IReadOnlyList<PaymentBySaleResponse>> GetPaymentsBySaleAsync()
    {
        return _reportRepository.GetPaymentsBySaleAsync();
    }
}
