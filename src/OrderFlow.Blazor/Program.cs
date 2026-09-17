using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrderFlow.Blazor;
using OrderFlow.Blazor.Options;
using OrderFlow.Blazor.Services;

var builder =
    WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>(
    "head::after");

var ordersApiUrl =
    builder.Configuration["ApiUrls:Orders"]
    ?? throw new InvalidOperationException(
        "Orders API URL is not configured.");

var inventoryApiUrl =
    builder.Configuration["ApiUrls:Inventory"]
    ?? throw new InvalidOperationException(
        "Inventory API URL is not configured.");

builder.Services.AddSingleton(
    new ApiUrlsOptions(
        new Uri(
            ordersApiUrl,
            UriKind.Absolute),
        new Uri(
            inventoryApiUrl,
            UriKind.Absolute)));

builder.Services.AddScoped(
    serviceProvider =>
        new OrdersApiClient(
            new HttpClient
            {
                BaseAddress = serviceProvider
                    .GetRequiredService<ApiUrlsOptions>()
                    .Orders
            }));

builder.Services.AddScoped(
    serviceProvider =>
        new InventoryApiClient(
            new HttpClient
            {
                BaseAddress = serviceProvider
                    .GetRequiredService<ApiUrlsOptions>()
                    .Inventory
            }));

await builder.Build().RunAsync();
