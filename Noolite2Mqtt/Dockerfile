#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Noolite2Mqtt.csproj", "Noolite2Mqtt/"]
COPY ["/Noolite2Mqtt.Plugins.Handlers/Noolite2Mqtt.Plugins.Handlers.csproj", "Noolite2Mqtt.Plugins.Handlers/"]
COPY ["/Noolite2Mqtt.Plugins.Devices/Noolite2Mqtt.Plugins.Devices.csproj", "Noolite2Mqtt.Plugins.Devices/"]
COPY ["/Noolite2Mqtt.Core.Plugins/Noolite2Mqtt.Core.Plugins.csproj", "Noolite2Mqtt.Core.Plugins/"]
COPY ["/Noolite2Mqtt.Plugins.NooLite/Noolite2Mqtt.Plugins.NooLite.csproj", "Noolite2Mqtt.Plugins.NooLite/"]
COPY ["/Noolite2Mqtt.Plugins.Timer/Noolite2Mqtt.Plugins.Timer.csproj", "Noolite2Mqtt.Plugins.Timer/"]
COPY ["/Noolite2Mqtt.Plugins.Mqtt/Noolite2Mqtt.Plugins.Mqtt.csproj", "Noolite2Mqtt.Plugins.Mqtt/"]
COPY ["/Noolite2Mqtt.Core.Infrastructure/Noolite2Mqtt.Core.Infrastructure.csproj", "Noolite2Mqtt.Core.Infrastructure/"]
RUN dotnet restore "Noolite2Mqtt.csproj"
COPY . .
WORKDIR "/src/Noolite2Mqtt"
RUN dotnet build "Noolite2Mqtt.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Noolite2Mqtt.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Noolite2Mqtt.dll"]