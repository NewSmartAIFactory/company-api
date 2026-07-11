alter table agents add column if not exists prompt text null;
alter table agents add column if not exists rules text null;
alter table agents add column if not exists workflow text null;
alter table agents add column if not exists memory_scope text null;
alter table agents add column if not exists tools_json jsonb not null default '[]'::jsonb;
alter table agents add column if not exists definition_path text null;
alter table agents add column if not exists definition_updated_at_utc timestamptz null;
