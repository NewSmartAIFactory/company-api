alter table reports add column if not exists summary text null;
alter table reports add column if not exists published_by text not null default 'PM';
alter table reports add column if not exists created_at_utc timestamptz not null default now();
create index if not exists ix_reports_project_created on reports (project_id, created_at_utc desc);
