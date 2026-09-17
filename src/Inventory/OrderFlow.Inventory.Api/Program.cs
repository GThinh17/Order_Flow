using OrderFlow.Inventory.Api.Endpoints;
using OrderFlow.Inventory.Application.Handler;
using OrderFlow.Inventory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string BlazorCorsPolicy = "BlazorClient";

var allowedOrigin =
    builder.Configuration["Cors:AllowedOrigin"]
    ?? throw new InvalidOperationException(
        "CORS allowed origin is not configured.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        BlazorCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins(allowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddOpenApi();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<GetStockHandler>();
builder.Services.AddScoped<AdjustStockHandler>();
builder.Services.AddScoped<ReserveOrderHandler>();
builder.Services.AddScoped<ConsumeReservationHandler>();
builder.Services.AddScoped<ReleaseReservationHandler>();


var app = builder.Build();

app.UseCors(BlazorCorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapStockEndpoints();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
