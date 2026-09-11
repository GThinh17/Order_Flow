FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["src/BuildingBlocks/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/BuildingBlocks/OrderFlow.Contracts/"]
COPY ["src/Inventory/OrderFlow.Inventory.Domain/OrderFlow.Inventory.Domain.csproj", "src/Inventory/OrderFlow.Inventory.Domain/"]
COPY ["src/Inventory/OrderFlow.Inventory.Application/OrderFlow.Inventory.Application.csproj", "src/Inventory/OrderFlow.Inventory.Application/"]
COPY ["src/Inventory/OrderFlow.Inventory.Infrastructure/OrderFlow.Inventory.Infrastructure.csproj", "src/Inventory/OrderFlow.Inventory.Infrastructure/"]
COPY ["src/Inventory/OrderFlow.Inventory.Api/OrderFlow.Inventory.Api.csproj", "src/Inventory/OrderFlow.Inventory.Api/"]

RUN dotnet restore \
    "src/Inventory/OrderFlow.Inventory.Api/OrderFlow.Inventory.Api.csproj"

COPY . .

WORKDIR /source/src/Inventory/OrderFlow.Inventory.Api

RUN dotnet publish \
    "OrderFlow.Inventory.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Inventory.Api.dll"]
