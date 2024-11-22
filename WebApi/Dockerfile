# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WebApi.csproj ./
RUN dotnet restore WebApi.csproj
COPY . ./
RUN dotnet build WebApi.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish WebApi.csproj -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=5012

COPY --from=publish /app/publish .

# Install utilities
RUN apt-get update && \
    apt-get install -y fswatch && \
    apt-get install -y bash && \
    apt-get install -y curl && \
    apt-get install -y tree && \
    apt-get install -y procps && \
    apt-get install -y iproute2 && \
    apt-get install -y net-tools && \
    apt-get install -y man-db && \
    rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "WebApi.dll"]
