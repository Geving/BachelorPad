FROM mcr.microsoft.com/dotnet/core/runtime:3.1
COPY bin/Release/netcoreapp3.1/linux-arm/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "NetCore.Docker.dll"]