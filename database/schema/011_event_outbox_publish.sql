alter table domain_events add column if not exists publish_attempts integer not null default 0;
alter table domain_events add column if not exists published_at_utc timestamptz null;
alter table domain_events add column if not exists last_publish_error text null;
