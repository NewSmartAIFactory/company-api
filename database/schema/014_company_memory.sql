create table if not exists memories (
    id uuid primary key,
    scope text not null,
    project_id text null references projects(id) on delete cascade,
    agent_id text null references agents(id) on delete set null,
    memory_type text not null,
    title text not null,
    content text not null,
    source text null,
    is_obsolete boolean not null default false,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz not null default now()
);
create index if not exists ix_memories_scope on memories(scope, is_obsolete, created_at_utc desc);
create index if not exists ix_memories_project on memories(project_id, created_at_utc desc);
