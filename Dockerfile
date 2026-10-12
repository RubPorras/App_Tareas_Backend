FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copiar solo el .csproj y restaurar (aprovecha el caché de capas)
COPY Backend.Api/Backend.Api.csproj Backend.Api/
RUN dotnet restore Backend.Api/Backend.Api.csproj

# 2. Copiar el resto del código y publicar
COPY . .
RUN dotnet publish Backend.Api/Backend.Api.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend.Api.dll"]