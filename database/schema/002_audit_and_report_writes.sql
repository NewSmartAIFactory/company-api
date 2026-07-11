create table if not exists audit_logs (
    id bigserial primary key,
    action text not null,
    entity_type text not null,
    entity_id text not null,
    actor text not null,
    previous_value text null,
    new_value text null,
    reason text null,
    created_at_utc timestamptz not null default now()
);

create index if not exists ix_audit_logs_created_at_utc
    on audit_logs (created_at_utc desc);

create index if not exists ix_audit_logs_entity
    on audit_logs (entity_type, entity_id);
