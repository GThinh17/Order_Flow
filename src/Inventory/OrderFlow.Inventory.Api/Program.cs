using OrderFlow.Inventory.Api.Endpoints;
using OrderFlow.Inventory.Application.Handler;
using OrderFlow.Inventory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<GetStockHandler>();
builder.Services.AddScoped<AdjustStockHandler>();
builder.Services.AddScoped<ReserveOrderHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapStockEndpoints();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
