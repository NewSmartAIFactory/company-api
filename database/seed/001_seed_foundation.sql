insert into agents (id, name, role, department, status, current_task) values
('ceo-assistant', 'CEO Assistant', 'Executive', 'Executive', 'Idle', null),
('pm', 'Project Manager', 'Management', 'Management', 'Working', 'Sprint 1.2 planning'),
('ba', 'Business Analyst', 'Product', 'Product', 'Idle', null),
('architect', 'Solution Architect', 'Engineering', 'Engineering', 'Idle', null),
('backend', 'Backend Agent', 'Engineering', 'Engineering', 'Working', 'Company API endpoints'),
('frontend', 'Frontend Agent', 'Engineering', 'Engineering', 'Idle', null),
('qc', 'QC Agent', 'Quality', 'Quality', 'Idle', null),
('devops', 'DevOps Agent', 'Operations', 'Operations', 'Idle', null),
('security', 'Security Reviewer', 'Quality', 'Quality', 'Idle', null),
('support', 'Support Agent', 'Customer Success', 'Customer Success', 'Idle', null)
on conflict (id) do nothing;

insert into projects (id, name, status, progress_percent, current_sprint) values
('company-os', 'AI Company OS', 'Foundation', 22, 'Sprint 1.2')
on conflict (id) do nothing;

insert into workflows (id, name, status) values
('software-delivery', 'Software Delivery', 'Draft')
on conflict (id) do nothing;

insert into workflow_steps (workflow_id, step_order, name)
select 'software-delivery', 1, 'CEO Request'
where not exists (select 1 from workflow_steps where workflow_id = 'software-delivery');

insert into workflow_steps (workflow_id, step_order, name) values
('software-delivery', 2, 'PM Planning'),
('software-delivery', 3, 'BA Analysis'),
('software-delivery', 4, 'Architecture'),
('software-delivery', 5, 'Development'),
('software-delivery', 6, 'QC'),
('software-delivery', 7, 'Deployment'),
('software-delivery', 8, 'Report')
on conflict do nothing;

insert into tasks (id, project_id, title, owner_agent_id, status, priority) values
('TASK-001', 'company-os', 'Create local infrastructure runtime', 'devops', 'Done', 'High'),
('TASK-002', 'company-os', 'Create Company API skeleton', 'backend', 'Done', 'High'),
('TASK-003', 'company-os', 'Create Company Dashboard skeleton', 'frontend', 'Done', 'High'),
('TASK-004', 'company-os', 'Add tasks, decisions, and reports endpoints', 'backend', 'Doing', 'High')
on conflict (id) do nothing;

insert into task_acceptance_criteria (task_id, criteria_order, criteria) values
('TASK-001', 1, 'Docker Compose starts PostgreSQL, Redis, Qdrant, and RabbitMQ'),
('TASK-002', 1, 'API exposes health, agents, projects, and workflows'),
('TASK-003', 1, 'Dashboard reads API and displays operating status'),
('TASK-004', 1, 'API exposes tasks, decisions, and reports')
on conflict do nothing;

insert into decisions (id, project_id, title, status, requested_by, impact, recommendation) values
('DEC-001', 'company-os', 'Use modular monolith before microservices', 'Approved', 'architect', 'Architecture', 'Start simple with module boundaries and split later only when needed'),
('DEC-002', 'company-os', 'Use .NET 10 on local machine', 'Approved', 'backend', 'Runtime', 'Target net10.0 because installed SDK/runtime is .NET 10')
on conflict (id) do nothing;

insert into reports (id, project_id, report_type, period, progress_percent) values
('RPT-001', 'company-os', 'Sprint', 'Sprint 1.1', 22)
on conflict (id) do nothing;

insert into report_items (report_id, item_type, item_order, content) values
('RPT-001', 'done', 1, 'GitHub repositories connected'),
('RPT-001', 'done', 2, 'Infrastructure is running'),
('RPT-001', 'done', 3, 'Company API is running'),
('RPT-001', 'done', 4, 'Dashboard is running'),
('RPT-001', 'doing', 1, 'Expanding API into tasks, decisions, and reports'),
('RPT-001', 'decisionsNeeded', 1, 'Confirm next priority: persistence or Telegram integration')
on conflict do nothing;