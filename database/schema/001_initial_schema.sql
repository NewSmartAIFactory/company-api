create table if not exists agents (
    id text primary key,
    name text not null,
    role text not null,
    department text not null,
    status text not null,
    current_task text null
);

create table if not exists projects (
    id text primary key,
    name text not null,
    status text not null,
    progress_percent integer not null default 0,
    current_sprint text null
);

create table if not exists workflows (
    id text primary key,
    name text not null,
    status text not null
);

create table if not exists workflow_steps (
    id bigserial primary key,
    workflow_id text not null references workflows(id) on delete cascade,
    step_order integer not null,
    name text not null
);

create table if not exists tasks (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    title text not null,
    owner_agent_id text not null references agents(id),
    status text not null,
    priority text not null
);

create table if not exists task_acceptance_criteria (
    id bigserial primary key,
    task_id text not null references tasks(id) on delete cascade,
    criteria_order integer not null,
    criteria text not null
);

create table if not exists decisions (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    title text not null,
    status text not null,
    requested_by text not null references agents(id),
    impact text not null,
    recommendation text not null
);

create table if not exists reports (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    report_type text not null,
    period text not null,
    progress_percent integer not null default 0
);

create table if not exists report_items (
    id bigserial primary key,
    report_id text not null references reports(id) on delete cascade,
    item_type text not null,
    item_order integer not null,
    content text not null
);