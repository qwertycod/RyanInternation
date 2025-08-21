# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Homework.sln ./
COPY Homework/*.csproj Homework/
COPY Homework.Helper/*.csproj Homework.Helper/
COPY Homework.Repository/*.csproj Homework.Repository/
COPY Homework.Services/*.csproj Homework.Services/


# Restore dependencies
RUN dotnet restore Homework/Homework.csproj

# Copy everything
COPY . .

# Publish the main API project
RUN dotnet publish Homework/Homework.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install CA certificates so HTTPS/TLS works for Azure SQL & Identity endpoints
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Homework.dll"]