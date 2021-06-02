# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /app

COPY * ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
RUN dotnet ef database update

# Build runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app

COPY --from=build /app/out .
EXPOSE 80

ENTRYPOINT ["dotnet", "WebApp.Tests.dll"]