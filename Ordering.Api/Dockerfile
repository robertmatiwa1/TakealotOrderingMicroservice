# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ../.. ./
RUN dotnet restore src/Ordering.Api/Ordering.Api.csproj
RUN dotnet publish src/Ordering.Api/Ordering.Api.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT [ "dotnet", "Ordering.Api.dll" ]
