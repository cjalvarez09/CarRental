FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CarRental.API/CarRental.API.csproj src/CarRental.API/
COPY src/Application/Application.csproj src/Application/
COPY src/CarRental.Domain/CarRental.Domain.csproj src/CarRental.Domain/
COPY src/CarRental.Infrastructure/CarRental.Infrastructure.csproj src/CarRental.Infrastructure/
RUN dotnet restore src/CarRental.API/CarRental.API.csproj

COPY src/ src/
RUN dotnet publish src/CarRental.API/CarRental.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "CarRental.API.dll"]
