create table if not exists approval_requests (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    title text not null,
    description text not null,
    requested_by text not null,
    status text not null default 'Pending',
    scope_impact text null,
    cost_impact text null,
    timeline_impact text null,
    security_impact text null,
    architecture_impact text null,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz not null default now()
);

create table if not exists approval_history (
    id bigserial primary key,
    approval_id text not null references approval_requests(id) on delete cascade,
    action text not null,
    actor text not null,
    comment text null,
    created_at_utc timestamptz not null default now()
);

create index if not exists ix_approval_requests_status on approval_requests (status, created_at_utc desc);
create index if not exists ix_approval_history_request on approval_history (approval_id, created_at_utc desc);
