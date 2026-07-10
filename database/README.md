# Database

Local PostgreSQL schema and seed scripts.

## Apply Manually

Use `psql` if available:

```powershell
psql "Host=localhost;Port=5432;Database=company_os;Username=postgres;Password=postgres" -f .\schema\001_initial_schema.sql
psql "Host=localhost;Port=5432;Database=company_os;Username=postgres;Password=postgres" -f .\seed\001_seed_foundation.sql
```

If `psql` is not installed locally, use Docker:

```powershell
docker exec -i smart-ai-factory-postgres psql -U postgres -d company_os < .\schema\001_initial_schema.sql
docker exec -i smart-ai-factory-postgres psql -U postgres -d company_os < .\seed\001_seed_foundation.sql
```