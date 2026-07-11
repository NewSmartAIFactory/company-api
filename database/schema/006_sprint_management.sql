create table if not exists sprints (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    name text not null,
    goal text not null,
    status text not null,
    start_date date null,
    end_date date null,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz not null default now()
);

alter table tasks add column if not exists sprint_id text null references sprints(id) on delete set null;
create index if not exists ix_sprints_project_status on sprints (project_id, status);
create index if not exists ix_tasks_sprint_id on tasks (sprint_id);

insert into sprints (id, project_id, name, goal, status)
values ('sprint-1-2', 'company-os', 'Sprint 1.2', 'Stabilize persistence and dashboard control actions', 'Completed')
on conflict (id) do nothing;
