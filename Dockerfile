FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
#FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.17-arm64v8 as base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ./ezviz-mqtt-systemd/ezviz-mqtt-systemd.csproj ./ezviz-mqtt-systemd/ezviz-mqtt-systemd.csproj
RUN dotnet restore "ezviz-mqtt-systemd/ezviz-mqtt-systemd.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ezviz.net.sln" -c Release

FROM build AS publish
RUN dotnet publish "ezviz-mqtt-systemd/ezviz-mqtt-systemd.csproj" -c Release -o /app/publish /p:PublishSingleFile=false

FROM base AS final
VOLUME /config
EXPOSE 8081
EXPOSE 8082

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ezviz-mqtt-systemd.dll"]
