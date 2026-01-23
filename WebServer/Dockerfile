FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY Presentation/Presentation.csproj Presentation/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Domain/Domain.csproj Domain/
COPY Protocol/Protocol.csproj Protocol/


RUN dotnet restore Presentation/Presentation.csproj


COPY . .


RUN dotnet publish Protocol/Protocol.csproj -c Release -o /app/contracts_out
RUN dotnet publish Presentation/Presentation.csproj -c Release -o /app/server_out


FROM base AS final
WORKDIR /app

COPY --from=build /app/contracts_out/ .
COPY --from=build /app/server_out/ .

ENTRYPOINT ["dotnet", "Presentation.dll"]
