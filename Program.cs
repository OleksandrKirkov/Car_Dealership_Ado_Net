using CarDealershipAdoNet.Database;
using CarDealershipAdoNet.Endpoints;
using CarDealershipAdoNet.Repositories;
using CarDealershipAdoNet.Services;
using MySqlConnector;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = "Server=test-mariadb.yacode.dev;Port=3307;Database=car_dealership;User ID=yacodedev_testing;Password=GpzA9rcpKuxrvUB3;Charset=utf8mb4;";

builder.Services.AddSingleton<IDbConnectionFactory>(
    new MySqlConnectionFactory(connectionString)
);

builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IReportService, ReportService>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapHealthEndpoints();
app.MapBrandEndpoints();
app.MapClientEndpoints();
app.MapCarEndpoints();
app.MapReportEndpoints();

app.Run();
