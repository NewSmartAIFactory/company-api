delete from workflow_steps
where id in (
    select id from (
        select id, row_number() over (partition by workflow_id, step_order order by id) as duplicate_number
        from workflow_steps
    ) duplicates
    where duplicate_number > 1
);

delete from task_acceptance_criteria
where id in (
    select id from (
        select id, row_number() over (partition by task_id, criteria_order order by id) as duplicate_number
        from task_acceptance_criteria
    ) duplicates
    where duplicate_number > 1
);

delete from report_items
where id in (
    select id from (
        select id, row_number() over (partition by report_id, item_type, item_order order by id) as duplicate_number
        from report_items
    ) duplicates
    where duplicate_number > 1
);

create unique index if not exists ux_workflow_steps_order
    on workflow_steps (workflow_id, step_order);

create unique index if not exists ux_task_acceptance_criteria_order
    on task_acceptance_criteria (task_id, criteria_order);

create unique index if not exists ux_report_items_order
    on report_items (report_id, item_type, item_order);
