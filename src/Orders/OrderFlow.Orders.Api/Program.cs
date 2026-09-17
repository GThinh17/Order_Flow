using OrderFlow.Orders.Api.Endpoints.Orders;
using OrderFlow.Orders.Application.Handler;
using OrderFlow.Orders.Infrastructure;

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


builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<ReservationSucceededHandler>();
builder.Services.AddScoped<ReservationFailedHandler>();
builder.Services.AddScoped<PaymentSucceededHandler>();
builder.Services.AddScoped<PaymentFailedHandler>();



builder.Services.AddInfrastructure(
    builder.Configuration);

var app = builder.Build();

app.UseCors(BlazorCorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapCreateOrderEndpoint();

app.MapControllers();

app.Run();

public partial class Program;
