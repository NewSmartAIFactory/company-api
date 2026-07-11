# company-api

Backend API for NewSmartAIFactory.

## Stack

- .NET 10 Web API
- Minimal APIs
- PostgreSQL, Redis, Qdrant, and RabbitMQ integration planned

## Run

```powershell
cd C:\data\DevApps\AICompany\company-api\src\Company.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --urls http://localhost:5000
```

## Endpoints

- `GET /`
- `GET /api/health`
- `GET /api/agents`
- `GET /api/agents/{id}`
- `POST /api/agents/sync`
- `GET /api/projects`
- `GET /api/workflows`
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `GET /api/decisions`
- `GET /api/decisions/{id}`
- `GET /api/reports`
- `GET /api/reports/{id}`
- `POST /api/reports`
- `GET /api/audit-logs?limit=50`
