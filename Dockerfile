# Use imagem runtime e SDK do .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Se houver .sln e vários projetos, copiar e restaurar separadamente melhora cache
COPY *.sln ./
# Copia todos os csproj (ajuste se necessário)
COPY **/*.csproj ./
RUN dotnet restore --no-cache || true

# Copia todo o código e publica
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Imagem final
FROM base AS final
WORKDIR /app

# Copia publicação
COPY --from=build /app/publish .

# Copia o entrypoint script e garante permissão de execução
COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Usa o script para localizar e executar o primeiro DLL publicado
ENTRYPOINT ["/entrypoint.sh"]
