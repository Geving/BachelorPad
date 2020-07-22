#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base #-buster-slim-arm32v7 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build #-buster-arm32v7 AS build
WORKDIR /src
COPY ["xComfortWingman/xComfortWingman.csproj", "xComfortWingman/"]
RUN dotnet restore "xComfortWingman/xComfortWingman.csproj"
COPY . .
WORKDIR "/src/xComfortWingman"
RUN dotnet build "xComfortWingman.csproj" -c Release -o /app/build #-r linux-arm

FROM build AS publish
RUN dotnet publish "xComfortWingman.csproj" -c Release -o /app/publish #-r linux-arm

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "xComfortWingman.dll"]
