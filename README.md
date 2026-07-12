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
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`
- `GET /api/sprints`
- `GET /api/sprints/{id}`
- `POST /api/sprints`
- `PUT /api/sprints/{id}`
- `DELETE /api/sprints/{id}`
- `GET /api/workflows`
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`
- `GET /api/approvals`
- `GET /api/approvals/{id}`
- `POST /api/approvals`
- `POST /api/approvals/{id}/actions`
- `GET /api/decisions`
- `GET /api/decisions/{id}`
- `GET /api/reports`
- `GET /api/reports/{id}`
- `POST /api/reports`
- `POST /api/reports/generate`
- `GET /api/events`
- `POST /api/events`
- `GET /api/workflow-runs`
- `POST /api/workflow-runs`
- `POST /api/workflow-runs/{id}/advance`
- `GET /api/agent-runs`
- `POST /api/agent-runs`
- `POST /api/agent-runs/{id}/complete`
- `GET /api/memory/search`
- `POST /api/memory`
- `PATCH /api/memory/{id}/obsolete`
- `GET /api/audit-logs?limit=50`
