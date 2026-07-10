# company-api

Backend API for NewSmartAIFactory.

## Stack

- .NET 9 Web API
- Minimal APIs
- PostgreSQL, Redis, Qdrant, and RabbitMQ integration planned

## Run

```powershell
cd C:\data\DevApps\AICompany\company-api\src\Company.Api
dotnet run --urls http://localhost:5000
```

## Endpoints

- `GET /`
- `GET /api/health`
- `GET /api/agents`
- `GET /api/projects`
- `GET /api/workflows`
