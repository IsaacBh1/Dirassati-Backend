#this is the hosting configuration
# this is the .net9 image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY . .
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o /out

# Use the .NET 9 runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .

# Expose the port for the API
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "Dirassati-Backend.dll"]
