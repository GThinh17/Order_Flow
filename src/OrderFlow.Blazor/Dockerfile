FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["src/OrderFlow.Blazor/OrderFlow.Blazor.csproj", "src/OrderFlow.Blazor/"]

RUN dotnet restore \
    "src/OrderFlow.Blazor/OrderFlow.Blazor.csproj"

COPY . .

WORKDIR /source/src/OrderFlow.Blazor

RUN dotnet publish \
    "OrderFlow.Blazor.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM nginx:alpine AS final

COPY ["src/OrderFlow.Blazor/nginx.conf", "/etc/nginx/conf.d/default.conf"]
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
