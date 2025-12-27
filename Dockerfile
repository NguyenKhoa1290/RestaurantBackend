# See https://docs.microsoft.com/dotnet/core/docker/building-net-docker-images for .NET Docker best practices
# Multi-stage build for ASP.NET Core


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./Restaurant_backend.csproj"
RUN dotnet publish "./Restaurant_backend.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS="http://+:5000"
ENTRYPOINT ["dotnet", "Restaurant_backend.dll"]
