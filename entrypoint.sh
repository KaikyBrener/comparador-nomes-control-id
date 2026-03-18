#!/bin/sh
set -e

# Procura o primeiro assembly .dll no diretório de trabalho
DLL=$(ls /app/*.dll 2>/dev/null | head -n 1)

# Se não achar em /app, tenta /app/publish (por segurança)
if [ -z "$DLL" ]; then
  DLL=$(ls /app/publish/*.dll 2>/dev/null | head -n 1)
fi

if [ -z "$DLL" ]; then
  echo "ERROR: No .dll found in /app or /app/publish"
  echo "Conteúdo de /app:"
  ls -la /app || true
  echo "Conteúdo de /app/publish:"
  ls -la /app/publish || true
  exit 1
fi

echo "Starting $DLL"
exec dotnet "$DLL"
