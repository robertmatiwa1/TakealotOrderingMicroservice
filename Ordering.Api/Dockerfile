# =============================================
# 🏗️  Stage 1: Build and Restore Dependencies
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY ["TakealotOrdering.sln", "./"]
COPY ["Ordering.Domain/Ordering.Domain.csproj", "Ordering.Domain/"]
COPY ["Ordering.Infrastructure/Ordering.Infrastructure.csproj", "Ordering.Infrastructure/"]
COPY ["Ordering.Application/Ordering.Application.csproj", "Ordering.Application/"]
COPY ["Ordering.Api/Ordering.Api.csproj", "Ordering.Api/"]

# Restore dependencies
RUN dotnet restore "Ordering.Api/Ordering.Api.csproj"

# Copy everything else
COPY . .

# Build the API in Release mode
WORKDIR "/src/Ordering.Api"
RUN dotnet build "Ordering.Api.csproj" -c Release -o /app/build

# =============================================
# 🚀 Stage 2: Publish the Application
# =============================================
FROM build AS publish
RUN dotnet publish "Ordering.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =============================================
# 🏁 Stage 3: Run the Application
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files from previous stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the app
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
