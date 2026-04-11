FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

# --- prerequisites for Blazor AOT publish , can probably be further optimised------------------------------
RUN apt-get update && \
    apt-get install -y --no-install-recommends python3 && \
    ln -s /usr/bin/python3 /usr/bin/python && \
    rm -rf /var/lib/apt/lists/*

COPY Boxty/ClientApp/ClientApp.sln ./Boxty/ClientApp/
COPY ./Boxty/ClientApp/Boxty.ClientApp.csproj ./Boxty/ClientApp/
COPY ./Boxty/SharedApp/Boxty.SharedApp.csproj ./Boxty/SharedApp/
COPY ./Boxty/NuGet.Config ./Boxty/
COPY ./nupkgs/ ./nupkgs/

RUN dotnet workload install wasm-tools --skip-manifest-update && \
    dotnet restore ./Boxty/ClientApp/Boxty.ClientApp.csproj --configfile ./Boxty/NuGet.Config
COPY . .
RUN find ./Boxty -type d \( -name bin -o -name obj \) -exec rm -rf {} + || true
WORKDIR /app/Boxty/ClientApp/
RUN dotnet restore "Boxty.ClientApp.csproj" --configfile /app/Boxty/NuGet.Config
RUN dotnet publish "Boxty.ClientApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM nginx:stable-perl
WORKDIR /app
EXPOSE 8080
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Add startup script to set environment header dynamically
COPY scripts/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

CMD ["/docker-entrypoint.sh"]