# 🔍 Comparador de Nomes - Control ID

## 📋 Descrição
Sistema web local em C# que compara duas listas de nomes:
- **Lista do sistema interno**
- **Lista do dispositivo Control ID**

Identifica nomes que existem no Control ID mas NÃO estão no sistema, permitindo sua remoção.

## 🚀 Como Rodar

### Pré-requisitos
- .NET 8.0 SDK instalado

### Instalação e Execução
```bash
# 1. Clonar o repositório
git clone https://github.com/KaikyBrener/comparador-nomes-control-id.git
cd comparador-nomes-control-id

# 2. Restaurar dependências
dotnet restore

# 3. Executar o projeto
dotnet run

# 4. Acessar no navegador
http://localhost:5000
```

## 📦 Gerar Executável (.exe)

```bash
dotnet publish -c Release -r win-x64 --self-contained

# Arquivo gerado em: bin/Release/net8.0/win-x64/publish/ComparadorNomes.exe
```

Depois é só rodar o .exe sem precisar do .NET instalado!

## ✨ Funcionalidades

✅ **Entrada Manual** - Cole as listas direto nos campos de texto  
✅ **Upload de Arquivos** - Envie arquivos .txt  
✅ **Comparação Inteligente** - Ignora case e espaços extras  
✅ **Resultado Visual** - Modal com lista de nomes para remover  
✅ **Copiar para Clipboard** - Um clique para copiar  
✅ **Download .txt** - Baixa arquivo pronto para usar  
✅ **Sem Armazenamento** - Tudo processado localmente  

## 🏗️ Arquitetura

- **Backend**: ASP.NET Core Minimal API (C#)
- **Frontend**: HTML + Bootstrap 5 + JavaScript
- **Comunicação**: HTTP POST
- **Execução**: Local, sem internet

## 📝 Como Usar

1. **Preencha ou envie** as duas listas (Sistema e Control ID)
2. **Clique em Comparar**
3. **Visualize** a lista de nomes para remover
4. **Copie** ou **Baixe** o resultado

## 🔐 Segurança

- Nenhum dado armazenado
- Execução 100% local
- Sem conexão com servidores externos

## 📌 Requisitos

- ✅ Comparação case-insensitive
- ✅ Normalização de espaços
- ✅ Validação de entrada
- ✅ Interface responsiva
- ✅ Processamento em tempo real

---

Desenvolvido em C#
