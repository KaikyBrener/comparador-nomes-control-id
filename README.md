# Comparador de Nomes — Control ID

![CI](https://github.com/KaikyBrener/comparador-nomes-control-id/actions/workflows/ci.yml/badge.svg)

Aplicação web em C# que compara a lista de nomes de um sistema interno com a lista de pessoas cadastradas em um dispositivo de controle de acesso (Control ID) e mostra quem existe no dispositivo mas não existe no sistema, ou seja, quem pode ser removido.

## Como funciona

1. Cole as duas listas nos campos de texto ou envie arquivos `.txt`.
2. Clique em **Comparar**.
3. O resultado traz duas listas:
   - **Remover:** nomes do dispositivo que não aparecem no sistema.
   - **Similares:** nomes que se parecem com algum nome do sistema (um contém o outro), para conferência manual.
4. Copie o resultado ou baixe em `.txt`.

A comparação ignora maiúsculas e minúsculas e espaços extras.

### Limpeza de listas

A rota `/limpar` prepara listas coladas de outros sistemas, extraindo só a coluna certa e removendo duplicados. Os formatos aceitos são `sistema`, `control`, `hikvision` e `intelbras`.

## Tecnologias

- Back-end: ASP.NET Core 8 (Minimal API), C#
- Front-end: HTML, Bootstrap 5 e JavaScript, servidos pela própria aplicação
- Docker (opcional)

## Como rodar

Requisito: .NET 8 SDK.

```bash
git clone https://github.com/KaikyBrener/comparador-nomes-control-id.git
cd comparador-nomes-control-id
dotnet run
```

Acesse a URL que o `dotnet run` mostrar no terminal (por padrão, `http://localhost:5000`).

### Com Docker

```bash
docker build -t comparador-nomes .
docker run -p 5000:5000 comparador-nomes
```

### Gerar executável (.exe)

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

O executável fica em `bin/Release/net8.0/win-x64/publish/ComparadorNomes.exe` e roda sem o .NET instalado.

## Rotas

| Método | Rota | Função |
|---|---|---|
| GET | `/` | Interface web |
| POST | `/comparar` | Compara as listas `sistema` e `control` |
| POST | `/limpar` | Normaliza uma lista (`dados` e `tipo`) |
| GET | `/log`, `/log/download` | Histórico das comparações da sessão |
| POST | `/log/limpar` | Apaga o histórico |

## Privacidade

- Nada é gravado em disco nem enviado a serviços externos.
- A aplicação guarda **em memória** o histórico das comparações (nome, ação e horário) até ser reiniciada, ou até você limpar em `/log/limpar`.
- As rotas de histórico não têm autenticação e o CORS está aberto. Por isso, **rode apenas localmente ou em rede confiável**. Não publique uma instância aberta na internet com dados reais de pessoas.
