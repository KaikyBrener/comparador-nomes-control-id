# Dockerfile (coloque na raiz do repo)
# Imagem base do runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
# Permite binding na 0.0.0.0:5000 via variável de ambiente
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia e restaura - otimiza cache se houver .csproj
COPY *.sln ./
COPY **/*.csproj ./
RUN dotnet restore --no-cache || true

# Copia todo o código e publica
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Imagem final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Usa shell para executar o primeiro dll encontrado (evita hardcode do nome do projeto)
ENTRYPOINT ["sh", "-c", "dotnet /app/publish/*.dll"]
