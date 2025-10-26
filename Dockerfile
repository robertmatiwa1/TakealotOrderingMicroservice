# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY . .
RUN dotnet restore "TakealotOrdering.sln"
RUN dotnet publish "Ordering.Api/Ordering.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5055
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
