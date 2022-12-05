#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ezviz-mqtt-docker/ezviz-mqtt-docker.csproj", "ezviz-mqtt-docker/"]
COPY ["ezviz-mqtt/ezviz-mqtt.csproj", "ezviz-mqtt/"]
COPY ["ezviz.net/ezviz.net.csproj", "ezviz.net/"]
RUN dotnet restore "ezviz-mqtt-docker/ezviz-mqtt-docker.csproj"
COPY . .
WORKDIR "/src/ezviz-mqtt-docker"
RUN dotnet build "ezviz-mqtt-docker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ezviz-mqtt-docker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ezviz-mqtt-docker.dll"]