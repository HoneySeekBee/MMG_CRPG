FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY WebServer/*.csproj WebServer/
COPY Application/*.csproj Application/
COPY Infrastructure/*.csproj Infrastructure/
COPY Domain/*.csproj Domain/

RUN dotnet restore WebServer/WebServer.csproj

COPY . .

RUN dotnet publish WebServer/WebServer.csproj -c Release -o /app/out


COPY WebServer/DataSeeds /app/out/DataSeeds

FROM base AS final
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "WebServer.dll"]
