FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY src/GastroLeinefeldeAPI/GastroLeinefeldeAPI.csproj GastroLeinefeldeAPI/
COPY src/GastroLeinefeldeAPI.Tests/GastroLeinefeldeAPI.Tests.csproj GastroLeinefeldeAPI.Tests/

RUN dotnet restore GastroLeinefeldeAPI/GastroLeinefeldeAPI.csproj

COPY src/ .

WORKDIR /src/GastroLeinefeldeAPI

RUN dotnet publish \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GastroLeinefeldeAPI.dll"]