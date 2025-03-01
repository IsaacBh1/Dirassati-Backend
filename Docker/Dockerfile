# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Ensure Git is installed (for pulling the latest code)
RUN apt-get update && apt-get install -y git

# Pull the latest version of the code from GitHub

RUN if [ -d ".git" ]; then git pull origin main; else git clone  https://github.com/IsaacBh1/Dirassati-Backend.git .; fi

# Restore dependencies
RUN dotnet restore

# Set environment to development mode
ENV ASPNETCORE_ENVIRONMENT=Development

# Build the project
RUN dotnet build -c Debug --no-restore
RUN dotnet publish -c Debug -o out

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Set environment to development mode (again, for runtime)
ENV ASPNETCORE_ENVIRONMENT=Development

# Copy the built application from the build stage
COPY --from=build /app/out .

# Expose ports for the app
EXPOSE 8080
EXPOSE 8081

# Start the application
ENTRYPOINT ["dotnet", "Dirassati-Backend.dll"]