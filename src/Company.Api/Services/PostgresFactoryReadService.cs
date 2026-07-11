using NewSmartAIFactory.CompanyApi.Models;
using Npgsql;
using System.Text.Json;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class PostgresFactoryReadService
{
    private readonly string _connectionString;

    public PostgresFactoryReadService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");
    }

    public async Task<IReadOnlyList<AgentSummary>> GetAgentsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select id, name, role, status, current_task
            from agents
            order by name;
            """;

        var items = new List<AgentSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AgentSummary(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                ParseAgentStatus(reader.GetString(3)),
                reader.IsDBNull(4) ? null : reader.GetString(4)
            ));
        }

        return items;
    }

    public async Task<AgentDetail?> GetAgentAsync(string id, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, name, role, department, status, current_task, prompt, rules, workflow, memory_scope,
                   tools_json::text, definition_path, definition_updated_at_utc
            from agents
            where id = @id;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new AgentDetail(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            ParseAgentStatus(reader.GetString(4)), reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            JsonSerializer.Deserialize<string[]>(reader.GetString(10)) ?? [],
            reader.IsDBNull(11) ? null : reader.GetString(11),
            reader.IsDBNull(12) ? null : reader.GetFieldValue<DateTimeOffset>(12));
    }

    public async Task<IReadOnlyList<ProjectSummary>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select id, name, status, progress_percent, current_sprint
            from projects
            order by name;
            """;

        var items = new List<ProjectSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjectSummary(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)
            ));
        }

        return items;
    }

    public async Task<ProjectDetail?> GetProjectAsync(string id, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, name, status, progress_percent, current_sprint, description, created_at_utc, updated_at_utc
            from projects
            where id = @id;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new ProjectDetail(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.GetFieldValue<DateTimeOffset>(6), reader.GetFieldValue<DateTimeOffset>(7));
    }

    public async Task<IReadOnlyList<SprintSummary>> GetSprintsAsync(string? projectId, CancellationToken cancellationToken)
    {
        const string sql = """
            select s.id, s.project_id, s.name, s.goal, s.status, s.start_date, s.end_date,
                   count(t.id)::int, count(t.id) filter (where t.status = 'Done')::int
            from sprints s left join tasks t on t.sprint_id = s.id
            where (@project_id is null or s.project_id = @project_id)
            group by s.id
            order by s.created_at_utc desc;
            """;
        var items = new List<SprintSummary>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add("project_id", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)projectId ?? DBNull.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) items.Add(new SprintSummary(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetFieldValue<DateOnly>(5), reader.IsDBNull(6) ? null : reader.GetFieldValue<DateOnly>(6), reader.GetInt32(7), reader.GetInt32(8)));
        return items;
    }

    public async Task<SprintDetail?> GetSprintAsync(string id, CancellationToken cancellationToken)
    {
        const string sprintSql = "select id, project_id, name, goal, status, start_date, end_date, created_at_utc, updated_at_utc from sprints where id = @id;";
        const string taskSql = "select id, project_id, title, owner_agent_id, status, priority from tasks where sprint_id = @id order by id;";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sprintSql, connection);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var values = (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
            reader.IsDBNull(5) ? (DateOnly?)null : reader.GetFieldValue<DateOnly>(5), reader.IsDBNull(6) ? (DateOnly?)null : reader.GetFieldValue<DateOnly>(6),
            reader.GetFieldValue<DateTimeOffset>(7), reader.GetFieldValue<DateTimeOffset>(8));
        await reader.CloseAsync();
        var backlog = new List<TaskSummary>();
        await using var taskCommand = new NpgsqlCommand(taskSql, connection); taskCommand.Parameters.AddWithValue("id", id);
        await using var taskReader = await taskCommand.ExecuteReaderAsync(cancellationToken);
        while (await taskReader.ReadAsync(cancellationToken)) backlog.Add(new TaskSummary(taskReader.GetString(0), taskReader.GetString(1), taskReader.GetString(2), taskReader.GetString(3), taskReader.GetString(4), taskReader.GetString(5), []));
        return new SprintDetail(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, values.Item7, values.Item8, values.Item9, backlog);
    }

    public async Task<IReadOnlyList<WorkflowSummary>> GetWorkflowsAsync(CancellationToken cancellationToken)
    {
        const string workflowSql = """
            select id, name, status
            from workflows
            order by name;
            """;

        const string stepsSql = """
            select name
            from workflow_steps
            where workflow_id = @workflow_id
            order by step_order;
            """;

        var items = new List<WorkflowSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var workflowCommand = new NpgsqlCommand(workflowSql, connection);
        await using var reader = await workflowCommand.ExecuteReaderAsync(cancellationToken);

        var workflows = new List<(string Id, string Name, string Status)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            workflows.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        await reader.CloseAsync();

        foreach (var workflow in workflows)
        {
            var steps = new List<string>();
            await using var stepsCommand = new NpgsqlCommand(stepsSql, connection);
            stepsCommand.Parameters.AddWithValue("workflow_id", workflow.Id);
            await using var stepsReader = await stepsCommand.ExecuteReaderAsync(cancellationToken);

            while (await stepsReader.ReadAsync(cancellationToken))
            {
                steps.Add(stepsReader.GetString(0));
            }

            items.Add(new WorkflowSummary(workflow.Id, workflow.Name, workflow.Status, steps));
        }

        return items;
    }

    public async Task<IReadOnlyList<TaskSummary>> GetTasksAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select id, project_id, title, owner_agent_id, status, priority
            from tasks
            order by id;
            """;

        const string criteriaSql = """
            select criteria
            from task_acceptance_criteria
            where task_id = @task_id
            order by criteria_order;
            """;

        var items = new List<TaskSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var tasks = new List<(string Id, string ProjectId, string Title, string OwnerAgentId, string Status, string Priority)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5)
            ));
        }

        await reader.CloseAsync();

        foreach (var task in tasks)
        {
            var criteria = new List<string>();
            await using var criteriaCommand = new NpgsqlCommand(criteriaSql, connection);
            criteriaCommand.Parameters.AddWithValue("task_id", task.Id);
            await using var criteriaReader = await criteriaCommand.ExecuteReaderAsync(cancellationToken);

            while (await criteriaReader.ReadAsync(cancellationToken))
            {
                criteria.Add(criteriaReader.GetString(0));
            }

            items.Add(new TaskSummary(task.Id, task.ProjectId, task.Title, task.OwnerAgentId, task.Status, task.Priority, criteria));
        }

        return items;
    }

    public async Task<TaskDetail?> GetTaskAsync(string id, CancellationToken cancellationToken)
    {
        const string taskSql = "select id, project_id, sprint_id, title, description, owner_agent_id, status, priority, due_date, created_at_utc, updated_at_utc from tasks where id=@id;";
        await using var connection = new NpgsqlConnection(_connectionString); await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(taskSql, connection); command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken); if (!await reader.ReadAsync(cancellationToken)) return null;
        var values = new { Id=reader.GetString(0), Project=reader.GetString(1), Sprint=reader.IsDBNull(2)?null:reader.GetString(2), Title=reader.GetString(3), Description=reader.IsDBNull(4)?null:reader.GetString(4), Owner=reader.GetString(5), Status=reader.GetString(6), Priority=reader.GetString(7), Due=reader.IsDBNull(8)?(DateOnly?)null:reader.GetFieldValue<DateOnly>(8), Created=reader.GetFieldValue<DateTimeOffset>(9), Updated=reader.GetFieldValue<DateTimeOffset>(10) }; await reader.CloseAsync();
        var criteria = new List<string>(); await using (var c = new NpgsqlCommand("select criteria from task_acceptance_criteria where task_id=@id order by criteria_order", connection)) { c.Parameters.AddWithValue("id", id); await using var r=await c.ExecuteReaderAsync(cancellationToken); while(await r.ReadAsync(cancellationToken)) criteria.Add(r.GetString(0)); }
        var dependencies = new List<string>(); await using (var c = new NpgsqlCommand("select depends_on_task_id from task_dependencies where task_id=@id order by depends_on_task_id", connection)) { c.Parameters.AddWithValue("id", id); await using var r=await c.ExecuteReaderAsync(cancellationToken); while(await r.ReadAsync(cancellationToken)) dependencies.Add(r.GetString(0)); }
        var activity = new List<AuditLogSummary>(); await using (var c = new NpgsqlCommand("select id,action,entity_type,entity_id,actor,previous_value,new_value,reason,created_at_utc from audit_logs where entity_type='task' and entity_id=@id order by created_at_utc desc limit 50", connection)) { c.Parameters.AddWithValue("id", id); await using var r=await c.ExecuteReaderAsync(cancellationToken); while(await r.ReadAsync(cancellationToken)) activity.Add(new AuditLogSummary(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.IsDBNull(5)?null:r.GetString(5),r.IsDBNull(6)?null:r.GetString(6),r.IsDBNull(7)?null:r.GetString(7),r.GetFieldValue<DateTimeOffset>(8))); }
        return new TaskDetail(values.Id,values.Project,values.Sprint,values.Title,values.Description,values.Owner,values.Status,values.Priority,values.Due,criteria,dependencies,values.Created,values.Updated,activity);
    }

    public async Task<IReadOnlyList<DecisionSummary>> GetDecisionsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select id, project_id, title, status, requested_by, impact, recommendation
            from decisions
            order by id;
            """;

        var items = new List<DecisionSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new DecisionSummary(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)
            ));
        }

        return items;
    }

    public async Task<IReadOnlyList<ReportSummary>> GetReportsAsync(CancellationToken cancellationToken)
    {
        const string reportSql = """
            select id, project_id, report_type, period, progress_percent, summary, published_by, created_at_utc
            from reports
            order by id desc;
            """;

        const string itemSql = """
            select item_type, content
            from report_items
            where report_id = @report_id
            order by item_type, item_order;
            """;

        var items = new List<ReportSummary>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(reportSql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var reports = new List<(string Id, string ProjectId, string ReportType, string Period, int ProgressPercent, string? Summary, string PublishedBy, DateTimeOffset CreatedAt)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            reports.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4), reader.IsDBNull(5) ? null : reader.GetString(5), reader.GetString(6), reader.GetFieldValue<DateTimeOffset>(7)));
        }

        await reader.CloseAsync();

        foreach (var report in reports)
        {
            var done = new List<string>();
            var doing = new List<string>();
            var blocked = new List<string>();
            var decisionsNeeded = new List<string>();

            await using var itemCommand = new NpgsqlCommand(itemSql, connection);
            itemCommand.Parameters.AddWithValue("report_id", report.Id);
            await using var itemReader = await itemCommand.ExecuteReaderAsync(cancellationToken);

            while (await itemReader.ReadAsync(cancellationToken))
            {
                var type = itemReader.GetString(0);
                var content = itemReader.GetString(1);

                switch (type)
                {
                    case "done":
                        done.Add(content);
                        break;
                    case "doing":
                        doing.Add(content);
                        break;
                    case "blocked":
                        blocked.Add(content);
                        break;
                    case "decisionsNeeded":
                        decisionsNeeded.Add(content);
                        break;
                }
            }

            items.Add(new ReportSummary(report.Id, report.ProjectId, report.ReportType, report.Period, report.ProgressPercent, done, doing, blocked, decisionsNeeded, report.Summary, report.PublishedBy, report.CreatedAt));
        }

        return items;
    }

    public async Task<IReadOnlyList<AuditLogSummary>> GetAuditLogsAsync(int limit, CancellationToken cancellationToken)
    {
        const string sql = """
            select id, action, entity_type, entity_id, actor, previous_value, new_value, reason, created_at_utc
            from audit_logs
            order by created_at_utc desc, id desc
            limit @limit;
            """;
        var items = new List<AuditLogSummary>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("limit", limit);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AuditLogSummary(
                reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7), reader.GetFieldValue<DateTimeOffset>(8)));
        }
        return items;
    }

    private static AgentStatus ParseAgentStatus(string status)
    {
        return Enum.TryParse<AgentStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : AgentStatus.Offline;
    }
}
