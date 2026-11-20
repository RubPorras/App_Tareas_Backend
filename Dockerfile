FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Restaurar solo con el .csproj (aprovecha la caché de capas)
COPY Backend.csproj .
RUN dotnet restore Backend.csproj

# 2. Copiar el resto y publicar
COPY . .
RUN dotnet publish Backend.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# curl para el healthcheck (como root, antes de cambiar de usuario)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Backend.dll"]