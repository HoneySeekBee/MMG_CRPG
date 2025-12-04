FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY WebServer/WebServer.csproj WebServer/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Domain/Domain.csproj Domain/
COPY Contracts/Contracts.csproj Contracts/


RUN dotnet restore WebServer/WebServer.csproj


COPY . .


RUN dotnet publish Contracts/Contracts.csproj -c Release -o /app/contracts_out
RUN dotnet publish WebServer/WebServer.csproj -c Release -o /app/server_out


FROM base AS final
WORKDIR /app

COPY --from=build /app/contracts_out/ .
COPY --from=build /app/server_out/ .

ENTRYPOINT ["dotnet", "WebServer.dll"]
