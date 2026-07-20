# Theatrical Players — Desafio Backend Aiko

Solução do [desafio de backend da Aiko](https://github.com/aikodigital/teste-backend-v4), seguindo a sugestão do **Theatrical Players Refactoring Kata**.

O domínio gera extratos de fatura de uma companhia de teatro (tragédia, comédia e histórico), em texto e XML, com design preparado para novos gêneros e formatos.

🎥 **Vídeo da apresentação:** https://youtu.be/N6z13OLUjnQ

---

## O que foi feito

### Escopo principal
- Refatoração do código legado pouco testável
- Gênero **history** (valor = tragédia + comédia)
- Extrato em **texto** e **XML**
- Design com **Strategy** para gêneros e formatters para saída
- Testes de aprovação (Verify) + testes unitários granulares

### Extras
- API REST com **Swagger**
- Processamento **assíncrono** (fila + worker) gerando XML em disco
- Persistência em **SQLite** (EF Core) do extrato, peças e linhas calculadas
- Logs estruturados com **Serilog** (console + arquivo rolling) para debug em produção
- Testes de integração da API

---

## Tecnologias

| Área | Tecnologia |
|------|------------|
| Runtime | .NET 10 / C# |
| Testes | xUnit, Verify (aprovação), Coverlet + ReportGenerator |
| API | ASP.NET Core, Swashbuckle (Swagger UI) |
| Persistência | EF Core + SQLite |
| Async | `Channel<T>` + `BackgroundService` |
| Logging | Serilog (console + arquivo) |
| Integração API | `WebApplicationFactory` |

---

## Arquitetura (visão rápida)

```
Invoice + Plays
      │
      ▼
StatementGenerator ──► GenrePricingFactory ──► Tragedy / Comedy / History
      │
      ▼
   Statement (modelo neutro)
      │
      ├── TextStatementFormatter
      └── XmlStatementFormatter
      │
      ▼ (API)
 POST /api/statements ──► SQLite + fila ──► worker ──► statements-output/{id}.xml
                                              │
                                              └── atualiza status/linhas/XML no banco
```

- **Gêneros:** cada um implementa `IGenrePricing`
- **Formatos:** cada um implementa `IStatementFormatter`
- **API:** enfileira o job, persiste no SQLite e responde `202`; o worker grava o XML e atualiza o registro
- **Logs:** `logs/api-YYYYMMDD.log` (+ console); em Development o nível sobe para Debug

Código principal em [`sugestão/`](sugestão/).

---

## Como rodar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

### Testes do domínio

```powershell
cd sugestão
dotnet test TheatricalPlayersRefactoringKata.Tests
```

### Testes da API

```powershell
cd sugestão
dotnet test TheatricalPlayersRefactoringKata.Api.Tests
```

### Toda a solution

```powershell
cd sugestão
dotnet test TheatricalPlayersRefactoringKata.sln
```

### Cobertura (HTML)

```powershell
cd sugestão
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
start coveragereport\index.html
```

### API + Swagger

```powershell
cd sugestão\TheatricalPlayersRefactoringKata.Api
dotnet run --launch-profile http
```

Abra: [http://localhost:5178/swagger](http://localhost:5178/swagger)

`POST /api/statements` enfileira o job → `GET /api/statements/{id}` consulta status → `GET /api/statements/{id}/xml` baixa o arquivo. Os XMLs ficam em `statements-output/`.

---

## Decisões relevantes

1. **Rede de segurança primeiro** — corrigir o teste legacy (line endings LF) antes de refatorar.
2. **Cálculo separado da apresentação** — modelo `Statement` + formatters.
3. **Strategy nos gêneros** — facilita novos tipos sem inchir a regra no printer.
4. **Assíncrono na borda** — domínio síncrono; fila/worker na API.
5. **IA como acelerador** — plano e revisão humanos; validação sempre por testes.

---

## Estrutura da solution

```
sugestão/
  TheatricalPlayersRefactoringKata/          # domínio
  TheatricalPlayersRefactoringKata.Tests/    # unitários + Verify
  TheatricalPlayersRefactoringKata.Api/      # REST + worker
  TheatricalPlayersRefactoringKata.Api.Tests/
```

O enunciado original do desafio permanece no histórico do repositório; este README descreve a solução entregue.
