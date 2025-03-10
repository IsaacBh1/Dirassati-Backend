FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app


COPY ["Dirassati-Backend.sln", "Dirassati-Backend.csproj", "./"]
RUN dotnet restore

COPY . .

RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app



COPY --from=build /app/out .


EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Dirassati-Backend.dll"]
