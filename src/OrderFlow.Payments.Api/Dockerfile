FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj", \
      "src/OrderFlow.Payments.Api/"]

RUN dotnet restore \
    "src/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj"

COPY . .

RUN dotnet publish \
    "src/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj" \
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