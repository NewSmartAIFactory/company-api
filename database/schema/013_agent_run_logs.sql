create table if not exists agent_runs (
    id uuid primary key,
    agent_id text not null references agents(id),
    task_id text null references tasks(id) on delete set null,
    project_id text null references projects(id) on delete set null,
    status text not null default 'Running',
    input_text text not null,
    output_text text null,
    duration_ms bigint null,
    files_touched jsonb not null default '[]'::jsonb,
    decision_requested text null,
    error text null,
    started_at_utc timestamptz not null default now(),
    completed_at_utc timestamptz null
);
create index if not exists ix_agent_runs_agent_started on agent_runs(agent_id, started_at_utc desc);
create index if not exists ix_agent_runs_status on agent_runs(status, started_at_utc desc);
