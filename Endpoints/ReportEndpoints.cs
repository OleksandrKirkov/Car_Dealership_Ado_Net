using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Services;

namespace CarDealershipAdoNet.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/reports")
            .WithTags("Reports");

        group.MapGet("/sales-join", async (IReportService reportService) =>
        {
            IReadOnlyList<SaleJoinResponse> result = await reportService.GetSalesJoinAsync();

            return Results.Ok(result);
        });

        group.MapGet("/available-cars", async (
            IReportService reportService,
            decimal maxPrice
        ) =>
        {
            IReadOnlyList<CarViewResponse> result =
                await reportService.GetAvailableCarsAsync(maxPrice);

            return Results.Ok(result);
        });

        group.MapGet("/cars-count-by-brand", async (IReportService reportService) =>
        {
            IReadOnlyList<CarsCountByBrandResponse> result =
                await reportService.GetCarsCountByBrandAsync();

            return Results.Ok(result);
        });

        group.MapGet("/total-sales", async (IReportService reportService) =>
        {
            TotalSalesResponse result = await reportService.GetTotalSalesAsync();

            return Results.Ok(result);
        });

        group.MapGet("/payments-by-sale", async (IReportService reportService) =>
        {
            IReadOnlyList<PaymentBySaleResponse> result =
                await reportService.GetPaymentsBySaleAsync();

            return Results.Ok(result);
        });
    }
}
