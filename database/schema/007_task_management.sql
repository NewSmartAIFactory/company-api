alter table tasks add column if not exists description text null;
alter table tasks add column if not exists due_date date null;
alter table tasks add column if not exists created_at_utc timestamptz not null default now();
alter table tasks add column if not exists updated_at_utc timestamptz not null default now();

create table if not exists task_dependencies (
    task_id text not null references tasks(id) on delete cascade,
    depends_on_task_id text not null references tasks(id) on delete restrict,
    primary key (task_id, depends_on_task_id),
    check (task_id <> depends_on_task_id)
);

create index if not exists ix_tasks_owner on tasks (owner_agent_id);
create index if not exists ix_tasks_project_status on tasks (project_id, status);
