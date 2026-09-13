using OrderFlow.Inventory.Api.Endpoints;
using OrderFlow.Inventory.Application.InventoryCommand;
using OrderFlow.Inventory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
builder.Services.AddScoped<GetStockHandler>();

builder.Services.AddScoped<AdjustStockHandler>();

app.MapStockEndpoints();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
