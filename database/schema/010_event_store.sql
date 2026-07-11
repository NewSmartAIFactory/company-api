create table if not exists domain_events (
    id uuid primary key,
    event_type text not null,
    aggregate_type text not null,
    aggregate_id text not null,
    project_id text null references projects(id) on delete set null,
    correlation_id text not null,
    actor text not null,
    payload jsonb not null default '{}'::jsonb,
    occurred_at_utc timestamptz not null,
    created_at_utc timestamptz not null default now()
);
create index if not exists ix_domain_events_occurred on domain_events (occurred_at_utc desc);
create index if not exists ix_domain_events_aggregate on domain_events (aggregate_type, aggregate_id);
create index if not exists ix_domain_events_project on domain_events (project_id, occurred_at_utc desc);
