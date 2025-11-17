FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IoTSystem.Shared/IoTSystem.Shared.csproj", "IoTSystem.Shared/"]
COPY ["IoTSensor/IoTSensor.csproj", "IoTSensor/"]
RUN dotnet restore "IoTSensor/IoTSensor.csproj"
COPY IoTSystem.Shared/ IoTSystem.Shared/
COPY IoTSensor/ IoTSensor/
WORKDIR "/src/IoTSensor"
RUN dotnet publish -c Release -o /app/publish
FROM base
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IoTSensor.dll"]