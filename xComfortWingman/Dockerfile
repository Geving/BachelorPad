#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["xComfortWingman/xComfort2MQTT.csproj", "xComfortWingman/"]
RUN dotnet restore "xComfortWingman/xComfort2MQTT.csproj"
COPY . .
WORKDIR "/src/xComfortWingman"
RUN dotnet build "xComfort2MQTT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "xComfort2MQTT.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "xComfort2MQTT.dll"]