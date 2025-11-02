# ===============================
# Build stage
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (cached layer)
COPY BookLendingService/BookLending.csproj ./BookLendingService/
RUN dotnet restore "BookLendingService/BookLending.csproj"

# Copy everything else and build
COPY .. .
WORKDIR /src/BookLendingService
RUN dotnet publish "BookLending.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ===============================
# Runtime stage
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set unified ports (matches VS + Compose)
ENV ASPNETCORE_URLS="https://+:8081;http://+:8080"
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "BookLending.dll"]
