# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Ensure Git is installed (for pulling the latest code)
RUN apt-get update && apt-get install -y git

# Pull the latest version of the code from GitHub
RUN git clone https://github.com/IsaacBh1/Dirassati-Backend.git .

# Restore dependencies
RUN dotnet restore

# Set environment to development mode
ENV ASPNETCORE_ENVIRONMENT=Development

# Build the project
RUN dotnet build -c Debug --no-restore



# Expose ports for the app
EXPOSE 8080
EXPOSE 8081

# Start the application
ENTRYPOINT ["dotnet", "run"]
