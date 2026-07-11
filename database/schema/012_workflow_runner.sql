create table if not exists workflow_runs (
    id uuid primary key,
    workflow_id text not null references workflows(id),
    project_id text not null references projects(id) on delete cascade,
    status text not null default 'Running',
    current_step_order integer not null default 1,
    initiated_by text not null,
    context text null,
    started_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz not null default now(),
    completed_at_utc timestamptz null
);
create index if not exists ix_workflow_runs_project on workflow_runs (project_id, updated_at_utc desc);
