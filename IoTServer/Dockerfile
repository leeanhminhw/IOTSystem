FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IoTSystem.Shared/IoTSystem.Shared.csproj", "IoTSystem.Shared/"]
COPY ["IoTServer/IoTServer.csproj", "IoTServer/"]
RUN dotnet restore "IoTServer/IoTServer.csproj"
COPY IoTSystem.Shared/ IoTSystem.Shared/
COPY IoTServer/ IoTServer/
WORKDIR "/src/IoTServer"
RUN dotnet publish -c Release -o /app/publish
FROM base
WORKDIR /app
RUN mkdir -p data
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IoTServer.dll"]