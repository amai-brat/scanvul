FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_HTTP_PORTS=5000 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["server/ScanVul.Server.API/ScanVul.Server.API.csproj", "server/ScanVul.Server.API/"]
COPY ["server/ScanVul.Server.Application/ScanVul.Server.Application.csproj", "server/ScanVul.Server.Application/"]
COPY ["ScanVul.Contracts/ScanVul.Contracts.csproj", "ScanVul.Contracts/"]
COPY ["server/ScanVul.Server.Domain/ScanVul.Server.Domain.csproj", "server/ScanVul.Server.Domain/"]
COPY ["server/ScanVul.Server.Infrastructure/ScanVul.Server.Infrastructure.csproj", "server/ScanVul.Server.Infrastructure/"]
RUN dotnet restore "server/ScanVul.Server.API/ScanVul.Server.API.csproj"

COPY ["server/ScanVul.Server.API/", "server/ScanVul.Server.API/"]
COPY ["server/ScanVul.Server.Application/", "server/ScanVul.Server.Application/"]
COPY ["server/ScanVul.Server.Domain/", "server/ScanVul.Server.Domain/"]
COPY ["server/ScanVul.Server.Infrastructure/", "server/ScanVul.Server.Infrastructure/"]
COPY ["ScanVul.Contracts/", "ScanVul.Contracts/"]

WORKDIR "/src/server/ScanVul.Server.API"
RUN dotnet build "./ScanVul.Server.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScanVul.Server.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScanVul.Server.API.dll"]
