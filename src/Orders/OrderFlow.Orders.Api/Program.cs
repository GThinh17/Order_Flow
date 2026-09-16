using OrderFlow.Orders.Api.Endpoints.Orders;
using OrderFlow.Orders.Application.Handler;
using OrderFlow.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<ReservationSucceededHandler>();
builder.Services.AddScoped<ReservationFailedHandler>();
builder.Services.AddScoped<PaymentSucceededHandler>();
builder.Services.AddScoped<PaymentFailedHandler>();

builder.Services.AddInfrastructure(
    builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapCreateOrderEndpoint();

app.Run();

public partial class Program;
