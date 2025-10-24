# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TakealotOrdering.sln", "./"]
COPY ["Ordering.Api/Ordering.Api.csproj", "Ordering.Api/"]
COPY ["Ordering.Application/Ordering.Application.csproj", "Ordering.Application/"]
COPY ["Ordering.Infrastructure/Ordering.Infrastructure.csproj", "Ordering.Infrastructure/"]
COPY ["Ordering.Domain/Ordering.Domain.csproj", "Ordering.Domain/"]

# Restore dependencies
RUN dotnet restore "TakealotOrdering.sln"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/Ordering.Api"
RUN dotnet build "Ordering.Api.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "Ordering.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5055
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
