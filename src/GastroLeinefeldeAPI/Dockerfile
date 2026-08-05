FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY src/GastroLeinefeldeAPI/GastroLeinefeldeAPI.csproj src/GastroLeinefeldeAPI/
RUN dotnet restore src/GastroLeinefeldeAPI/GastroLeinefeldeAPI.csproj

# Copy everything else and build
COPY src/ .
WORKDIR /app/src/GastroLeinefeldeAPI
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/src/GastroLeinefeldeAPI/out .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "GastroLeinefeldeAPI.dll"]