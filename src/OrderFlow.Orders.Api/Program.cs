using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Infrastructure.Health;
using OrderFlow.Orders.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var databaseConnectionString =
    builder.Configuration["Database:ConnectionString"];

if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    throw new InvalidOperationException(
        "Database connection string is not configured.");
}

var pulsarAdminUrl = builder.Configuration["Pulsar:AdminUrl"];

if (!Uri.TryCreate(
        pulsarAdminUrl,
        UriKind.Absolute,
        out var pulsarAdminUri))
{
    throw new InvalidOperationException(
        "Pulsar admin URL is missing or invalid.");
}

builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(databaseConnectionString));

builder.Services.AddHttpClient(
    PulsarHealthCheck.HttpClientName,
    client =>
    {
        client.BaseAddress = pulsarAdminUri;
        client.Timeout = TimeSpan.FromSeconds(3);
    });

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("database")
    .AddCheck<PulsarHealthCheck>("pulsar");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.Run();
