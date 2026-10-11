# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["project_1.csproj", "./"]
RUN dotnet restore "./project_1.csproj"

COPY . .
RUN dotnet publish "project_1.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Run the published application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "project_1.dll"]