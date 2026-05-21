# ETAPA DE CONSTRUCCION (BUILD)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Banco.API/Banco.API.csproj", "Banco.API/"]
COPY ["Banco.Aplicacion/Banco.Aplicacion.csproj", "Banco.Aplicacion/"]
COPY ["Banco.Dominio/Banco.Dominio.csproj", "Banco.Dominio/"]
COPY ["Banco.Infraestructura/Banco.Infraestructura.csproj", "Banco.Infraestructura/"]
RUN dotnet restore "Banco.API/Banco.API.csproj"

COPY . .
WORKDIR "/src/Banco.API"
RUN dotnet publish "Banco.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA DE PRODUCCION (RUNTIME)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "Banco.API.dll" ]
