FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IoTSystem.Shared/IoTSystem.Shared.csproj", "IoTSystem.Shared/"]
COPY ["IoTWebAPI/IoTWebAPI.csproj", "IoTWebAPI/"]
RUN dotnet restore "IoTWebAPI/IoTWebAPI.csproj"
COPY IoTSystem.Shared/ IoTSystem.Shared/
COPY IoTWebAPI/ IoTWebAPI/
WORKDIR "/src/IoTWebAPI"
RUN dotnet publish -c Release -o /app/publish
FROM base
WORKDIR /app
RUN mkdir -p data
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "IoTWebAPI.dll"]