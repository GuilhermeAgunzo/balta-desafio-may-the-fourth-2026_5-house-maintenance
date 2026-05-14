<img width="1280" height="630" alt="banner" src="https://github.com/user-attachments/assets/eb2f345f-7b28-41d0-b374-6336dc8f8f75" />

## 🚀 May The Fourth 2026 - Desafio 5

Aplicação fullstack em **.NET 10** para organizar pequenos reparos domésticos com ajuda de IA. O usuário informa tarefas como trocar lâmpada, fixar quadro e limpar ralo, e o sistema gera uma ordem de execução que reaproveita o mesmo kit de ferramentas em sequência, reduzindo trocas de contexto e bagunça.

## O que foi implementado

- **API ASP.NET Core** com endpoint para gerar o plano e endpoint para consultar histórico
- **Camada de aplicação** com normalização, deduplicação, dicas de toolkit e validação do contrato da IA
- **Camada de IA** com **Microsoft Agent Framework + OpenAI** retornando saída estruturada
- **Persistência em SQLite** com EF Core para salvar missões e planos gerados
- **Frontend em Blazor WebAssembly** com temática sci-fi/Star Wars inspirada no `design.md`
- **Aspire 13.3.1** com AppHost para orquestrar API e frontend
- **Testes** de aplicação, API e interface Blazor

## Estrutura da solução

| Projeto | Responsabilidade |
| --- | --- |
| `HouseMaintenance.API` | Endpoints HTTP, composição da aplicação e tratamento de erros |
| `HouseMaintenance.Application` | Regras de negócio, preparação das tarefas e validação do plano |
| `HouseMaintenance.Core` | Entidades, DTOs e contratos |
| `HouseMaintenance.Infra` | `DbContext`, SQLite e repositório |
| `HouseMaintenance.AI` | Prompt e agente do Microsoft Agent Framework com OpenAI |
| `HouseMaintenance.Web` | Host ASP.NET Core do frontend |
| `HouseMaintenance.Web.Client` | UI Blazor WebAssembly |
| `HouseMaintenance.AppHost` | Orquestração com Aspire |
| `HouseMaintenance.ServiceDefaults` | Telemetria, health checks e service discovery do Aspire |

## Stack

- .NET 10
- ASP.NET Core Minimal API
- Blazor WebAssembly
- Microsoft Agent Framework (`Microsoft.Agents.AI.OpenAI`)
- OpenAI
- Entity Framework Core + SQLite
- Aspire 13.3.1
- xUnit + bUnit

## Design

O frontend segue os tokens e princípios descritos em `design.md`, reinterpretando o visual **NASA-punk** para um painel com clima de **cockpit Star Wars**:

- painéis rígidos e chanfrados
- badges/HUD com tipografia técnica
- contraste entre azul profundo, dourado, verde e vermelho
- foco em painel de missão, não em feed

## Como executar

### 1. Configurar a chave da OpenAI

Defina a variável de ambiente abaixo antes de gerar planos:

```powershell
$env:OpenAI__ApiKey = "sua-chave"
```

Opcionalmente, troque o modelo:

```powershell
$env:OpenAI__Model = "gpt-5.4-mini"
```

Também é possível ajustar o timeout de rede do cliente OpenAI:

```powershell
$env:OpenAI__TimeoutSeconds = "90"
```

Se o plano estiver levando mais tempo para ser gerado, o host Web também aceita aumentar o timeout do proxy same-origin para a API:

```powershell
$env:Api__TimeoutSeconds = "180"
```

> Sem `OpenAI__ApiKey`, a aplicação sobe normalmente, mas a geração do plano retorna erro informando a configuração ausente.

### 2. Executar com Aspire

```powershell
dotnet run --project .\src\HouseMaintenance.AppHost\
```

### 3. Executar manualmente

API:

```powershell
dotnet run --project .\src\HouseMaintenance.API\
```

Frontend:

```powershell
dotnet run --project .\src\HouseMaintenance.Web\HouseMaintenance.Web\
```

## Endpoints principais

### Gerar plano

`POST /api/maintenance-plans`

```json
{
  "tasks": [
    "Trocar lâmpada do corredor",
    "Fixar quadro da sala",
    "Limpar ralo do banheiro"
  ]
}
```

### Histórico

`GET /api/maintenance-plans/history?take=6`

### Detalhe da missão

`GET /api/maintenance-plans/{missionId}`

## Testes

Executar build:

```powershell
dotnet build HouseMaintenance.slnx --tl:off -v minimal
```

Executar testes:

```powershell
dotnet test HouseMaintenance.slnx --tl:off -v minimal
```

## Estratégia de commits sugerida

Para manter a entrega organizada, a solução foi estruturada para permitir commits por fatia funcional:

1. bootstrap da solução e Aspire
2. domínio e aplicação
3. agente IA
4. persistência
5. API
6. frontend
7. testes e documentação

## Observações de arquitetura

- `HouseMaintenance.AI` **não concentra regras de negócio**; ele expõe apenas o agente e o prompt.
- A camada `HouseMaintenance.Application` decide como preparar os reparos, validar a resposta e persistir o resultado.
- O frontend consome o backend por um **proxy same-origin** no host Web, enquanto o AppHost usa service discovery do Aspire para conectar os projetos.
