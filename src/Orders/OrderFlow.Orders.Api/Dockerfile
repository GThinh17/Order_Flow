FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["src/BuildingBlocks/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/BuildingBlocks/OrderFlow.Contracts/"]
COPY ["src/Orders/OrderFlow.Orders.Domain/OrderFlow.Orders.Domain.csproj", "src/Orders/OrderFlow.Orders.Domain/"]
COPY ["src/Orders/OrderFlow.Orders.Application/OrderFlow.Orders.Application.csproj", "src/Orders/OrderFlow.Orders.Application/"]
COPY ["src/Orders/OrderFlow.Orders.Infrastructure/OrderFlow.Orders.Infrastructure.csproj", "src/Orders/OrderFlow.Orders.Infrastructure/"]
COPY ["src/Orders/OrderFlow.Orders.Api/OrderFlow.Orders.Api.csproj", "src/Orders/OrderFlow.Orders.Api/"]

RUN dotnet restore \
    "src/Orders/OrderFlow.Orders.Api/OrderFlow.Orders.Api.csproj"

COPY . .

WORKDIR /source/src/Orders/OrderFlow.Orders.Api

RUN dotnet publish \
    "OrderFlow.Orders.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Orders.Api.dll"]
