FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["src/BuildingBlocks/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/BuildingBlocks/OrderFlow.Contracts/"]
COPY ["src/Payments/OrderFlow.Payments.Domain/OrderFlow.Payments.Domain.csproj", "src/Payments/OrderFlow.Payments.Domain/"]
COPY ["src/Payments/OrderFlow.Payments.Application/OrderFlow.Payments.Application.csproj", "src/Payments/OrderFlow.Payments.Application/"]
COPY ["src/Payments/OrderFlow.Payments.Infrastructure/OrderFlow.Payments.Infrastructure.csproj", "src/Payments/OrderFlow.Payments.Infrastructure/"]
COPY ["src/Payments/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj", "src/Payments/OrderFlow.Payments.Api/"]

RUN dotnet restore \
    "src/Payments/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj"

COPY . .

WORKDIR /source/src/Payments/OrderFlow.Payments.Api

RUN dotnet publish \
    "OrderFlow.Payments.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Payments.Api.dll"]
