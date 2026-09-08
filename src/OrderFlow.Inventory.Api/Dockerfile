FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OrderFlow.Inventory.Api/OrderFlow.Inventory.Api.csproj", \
      "src/OrderFlow.Inventory.Api/"]

RUN dotnet restore \
    "src/OrderFlow.Inventory.Api/OrderFlow.Inventory.Api.csproj"

COPY . .

RUN dotnet publish \
    "src/OrderFlow.Inventory.Api/OrderFlow.Inventory.Api.csproj" \
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