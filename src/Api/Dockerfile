# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source
# Copy the API project and everything it references so restore resolves cleanly.
COPY ["src/Api/PaperlessLabelGenerator.csproj", "src/Api/"]
COPY ["src/Core/PaperlessLabelGenerator.Core.csproj", "src/Core/"]
COPY ["src/Aspire/ServiceDefaults/PaperlessLabelGenerator.ServiceDefaults.csproj", "src/Aspire/ServiceDefaults/"]
RUN dotnet restore "./src/Api/PaperlessLabelGenerator.csproj"
COPY . .
WORKDIR "/source/src/Api"
RUN dotnet build "./PaperlessLabelGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PaperlessLabelGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaperlessLabelGenerator.dll"]