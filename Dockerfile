#Build using SDK 8.0 image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything, Solution and cs proj files
COPY FileIntake.sln .
COPY FileIntake/FileIntake.csproj FileIntake/
COPY FileIntake.Tests/FileIntake.Tests.csproj FileIntake.Tests/

# Restore
RUN dotnet restore

# Copy remaining files and build, outputs DLLs -> /app/publish
COPY . .
WORKDIR /src/FileIntake
RUN dotnet publish -c Release -o /app/publish

# Smaller Runtime-only environment (excluding compiler)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Setting up the runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FileIntake.dll"]
