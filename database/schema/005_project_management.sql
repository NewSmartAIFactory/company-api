alter table projects add column if not exists description text null;
alter table projects add column if not exists created_at_utc timestamptz not null default now();
alter table projects add column if not exists updated_at_utc timestamptz not null default now();

create index if not exists ix_projects_status on projects (status);
