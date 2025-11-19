# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyecto
COPY Broxchain.sln ./
COPY FondoInversion/FondoInversion.csproj ./FondoInversion/
RUN dotnet restore ./FondoInversion/FondoInversion.csproj

# Copiar todo el código
COPY . .

# Publicar la FondoInversion
RUN dotnet publish ./FondoInversion/FondoInversion.csproj -c Release -o /FondoInversion/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /FondoInversion


# Copiar artefactos de la build
COPY --from=build /FondoInversion/publish .

# Copiar base de datos y scripts al contenedor (opcional si se descargan desde bucket)
COPY DB ./DB

# Descargar DB desde bucket (si existe), luego ejecutar app
ENTRYPOINT ["dotnet", "FondoInversion.dll"]
