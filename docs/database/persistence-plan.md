# Persistence Plan

## Goal

Move Company API from in-memory placeholder data to PostgreSQL-backed operating data.

## Database

PostgreSQL database:

```text
company_os
```

Connection string:

```text
Host=localhost;Port=5432;Database=company_os;Username=postgres;Password=postgres
```

## Phase 1 Tables

- agents
- projects
- workflows
- workflow_steps
- tasks
- decisions
- reports
- report_items

## Phase 1 API Changes

Replace `FactoryStateService` hardcoded data with:

- repository interfaces
- PostgreSQL implementations
- seed data script

## Technical Options

### Option A: EF Core

Pros:

- Fast implementation
- Strong .NET ecosystem
- Migrations built in

Cons:

- Adds ORM abstraction
- Requires package restore from NuGet

### Option B: Dapper

Pros:

- Simple SQL control
- Lightweight
- Good for explicit schema

Cons:

- More manual mapping
- No migrations built in

## Current implementation

The API currently uses explicit Npgsql SQL services and ordered SQL files under
`database/schema`. PostgreSQL is the source of truth; Qdrant is a derived index
for semantic memory search.

## Acceptance Criteria

- Database schema can be created locally
- Seed data matches current dashboard demo data
- API reads agents/projects/workflows/tasks/decisions/reports from PostgreSQL
- Dashboard behavior remains unchanged
