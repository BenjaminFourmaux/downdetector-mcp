# This stage is used when running from VS in fast mode (default for debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DowdetectorMCP.Server/DowdetectorMCP.Server.csproj", "DowdetectorMCP.Server/"]
COPY ["DowndetectorMCP.API/DowndetectorMCP.API.csproj", "DowndetectorMCP.API/"]
RUN dotnet restore "./DowdetectorMCP.Server/DowdetectorMCP.Server.csproj"
COPY . .
WORKDIR "/src/DowdetectorMCP.Server"
RUN dotnet build "./DowdetectorMCP.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied in the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DowdetectorMCP.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Install Playwright Chromium browser in the publish output
RUN pwsh /app/publish/playwright.ps1 install chromium

# This stage is used in production or when running from VS in normal mode (default when debug configuration is not used)
FROM base AS final

# Install Playwright Chromium dependencies (as root)
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libnspr4 \
    libdbus-1-3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libatspi2.0-0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2t64 \
    libwayland-client0 \
    fonts-liberation \
    fonts-noto-color-emoji \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app/publish .

# Copy Playwright browsers from the build stage
COPY --from=publish /root/.cache/ms-playwright /home/app/.cache/ms-playwright

# Fix permissions on Playwright node driver and browsers
RUN chmod -R +x /app/.playwright && \
    chown -R $APP_UID /home/app/.cache/ms-playwright
    
USER $APP_UID
ENTRYPOINT ["dotnet", "DowdetectorMCP.Server.dll"]