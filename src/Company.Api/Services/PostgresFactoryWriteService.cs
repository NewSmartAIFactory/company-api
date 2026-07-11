using NewSmartAIFactory.CompanyApi.Models;
using Npgsql;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class PostgresFactoryWriteService
{
    private static readonly HashSet<string> AllowedTaskStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Todo", "Doing", "Done", "Blocked"
    };

    private readonly string _connectionString;

    public PostgresFactoryWriteService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");
    }

    public async Task<bool> UpdateTaskStatusAsync(string id, string status, string actor, string? reason, CancellationToken cancellationToken)
    {
        if (!AllowedTaskStatuses.Contains(status))
            throw new ArgumentException($"Unsupported task status: {status}", nameof(status));

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var previousStatus = await GetStatusAsync(connection, transaction, "tasks", id, cancellationToken);
        if (previousStatus is null) return false;
        if (previousStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
        {
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        await using (var command = new NpgsqlCommand("update tasks set status = @status where id = @id;", connection, transaction))
        {
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("status", status);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await InsertAuditAsync(connection, transaction, "task.status.changed", "task", id, actor, previousStatus, status, reason, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task<bool> ApproveDecisionAsync(string id, string actor, string? reason, CancellationToken cancellationToken) =>
        SetDecisionStatusAsync(id, "Approved", actor, reason, cancellationToken);

    public Task<bool> RejectDecisionAsync(string id, string actor, string? reason, CancellationToken cancellationToken) =>
        SetDecisionStatusAsync(id, "Rejected", actor, reason, cancellationToken);

    private async Task<bool> SetDecisionStatusAsync(string id, string status, string actor, string? reason, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var previousStatus = await GetStatusAsync(connection, transaction, "decisions", id, cancellationToken);
        if (previousStatus is null) return false;
        if (previousStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
        {
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        await using (var command = new NpgsqlCommand("update decisions set status = @status where id = @id;", connection, transaction))
        {
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("status", status);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await InsertAuditAsync(connection, transaction, $"decision.{status.ToLowerInvariant()}", "decision", id, actor, previousStatus, status, reason, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<string> CreateReportAsync(CreateReportRequest request, CancellationToken cancellationToken)
    {
        var id = $"RPT-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string reportSql = "insert into reports (id, project_id, report_type, period, progress_percent) values (@id, @project_id, @report_type, @period, @progress_percent);";
        await using (var command = new NpgsqlCommand(reportSql, connection, transaction))
        {
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("project_id", request.ProjectId.Trim());
            command.Parameters.AddWithValue("report_type", request.ReportType.Trim());
            command.Parameters.AddWithValue("period", request.Period.Trim());
            command.Parameters.AddWithValue("progress_percent", request.ProgressPercent);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await InsertReportItemsAsync(connection, transaction, id, "done", request.Done, cancellationToken);
        await InsertReportItemsAsync(connection, transaction, id, "doing", request.Doing, cancellationToken);
        await InsertReportItemsAsync(connection, transaction, id, "blocked", request.Blocked, cancellationToken);
        await InsertReportItemsAsync(connection, transaction, id, "decisionsNeeded", request.DecisionsNeeded, cancellationToken);
        await InsertAuditAsync(connection, transaction, "report.published", "report", id, request.PublishedBy ?? "PM", null, request.Period, null, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return id;
    }

    public async Task<string> CreateProjectAsync(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id!.Trim();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        const string sql = """
            insert into projects (id, name, status, progress_percent, current_sprint, description, created_at_utc, updated_at_utc)
            values (@id, @name, @status, @progress, @sprint, @description, now(), now());
            """;
        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            AddProjectParameters(command, id, request);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await InsertAuditAsync(connection, transaction, "project.created", "project", id, request.Actor ?? "CEO", null, request.Status, request.Description, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdateProjectAsync(string id, SaveProjectRequest request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var previousStatus = await GetStatusAsync(connection, transaction, "projects", id, cancellationToken);
        if (previousStatus is null) return false;
        const string sql = """
            update projects set name = @name, status = @status, progress_percent = @progress,
                current_sprint = @sprint, description = @description, updated_at_utc = now()
            where id = @id;
            """;
        await using (var command = new NpgsqlCommand(sql, connection, transaction))
        {
            AddProjectParameters(command, id, request);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await InsertAuditAsync(connection, transaction, "project.updated", "project", id, request.Actor ?? "CEO", previousStatus, request.Status, request.Description, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(string id, string actor, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var previousStatus = await GetStatusAsync(connection, transaction, "projects", id, cancellationToken);
        if (previousStatus is null) return false;
        await InsertAuditAsync(connection, transaction, "project.deleted", "project", id, actor, previousStatus, null, null, cancellationToken);
        const string sql = """
            delete from projects p where p.id = @id
              and not exists (select 1 from tasks where project_id = p.id)
              and not exists (select 1 from decisions where project_id = p.id)
              and not exists (select 1 from reports where project_id = p.id);
            """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("id", id);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            throw new InvalidOperationException("Project has related work and must be archived instead of deleted.");
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static void AddProjectParameters(NpgsqlCommand command, string id, SaveProjectRequest request)
    {
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("name", request.Name.Trim());
        command.Parameters.AddWithValue("status", request.Status.Trim());
        command.Parameters.AddWithValue("progress", request.ProgressPercent);
        command.Parameters.AddWithValue("sprint", (object?)request.CurrentSprint?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("description", (object?)request.Description?.Trim() ?? DBNull.Value);
    }

    public async Task<string> CreateSprintAsync(SaveSprintRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id!.Trim();
        await using var connection = new NpgsqlConnection(_connectionString); await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        const string sql = "insert into sprints (id, project_id, name, goal, status, start_date, end_date) values (@id, @project, @name, @goal, @status, @start, @end);";
        await using (var command = new NpgsqlCommand(sql, connection, transaction)) { AddSprintParameters(command, id, request); await command.ExecuteNonQueryAsync(cancellationToken); }
        await InsertAuditAsync(connection, transaction, "sprint.created", "sprint", id, request.Actor ?? "PM", null, request.Status, request.Goal, cancellationToken);
        await transaction.CommitAsync(cancellationToken); return id;
    }

    public async Task<bool> UpdateSprintAsync(string id, SaveSprintRequest request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString); await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var previousStatus = await GetStatusAsync(connection, transaction, "sprints", id, cancellationToken); if (previousStatus is null) return false;
        const string sql = "update sprints set project_id=@project, name=@name, goal=@goal, status=@status, start_date=@start, end_date=@end, updated_at_utc=now() where id=@id;";
        await using (var command = new NpgsqlCommand(sql, connection, transaction)) { AddSprintParameters(command, id, request); await command.ExecuteNonQueryAsync(cancellationToken); }
        await InsertAuditAsync(connection, transaction, "sprint.updated", "sprint", id, request.Actor ?? "PM", previousStatus, request.Status, request.Goal, cancellationToken);
        await transaction.CommitAsync(cancellationToken); return true;
    }

    public async Task<bool> DeleteSprintAsync(string id, string actor, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString); await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var status = await GetStatusAsync(connection, transaction, "sprints", id, cancellationToken); if (status is null) return false;
        await using var command = new NpgsqlCommand("delete from sprints s where id=@id and not exists (select 1 from tasks where sprint_id=s.id);", connection, transaction); command.Parameters.AddWithValue("id", id);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0) throw new InvalidOperationException("Sprint has backlog tasks and cannot be deleted.");
        await InsertAuditAsync(connection, transaction, "sprint.deleted", "sprint", id, actor, status, null, null, cancellationToken);
        await transaction.CommitAsync(cancellationToken); return true;
    }

    private static void AddSprintParameters(NpgsqlCommand command, string id, SaveSprintRequest request)
    {
        command.Parameters.AddWithValue("id", id); command.Parameters.AddWithValue("project", request.ProjectId.Trim());
        command.Parameters.AddWithValue("name", request.Name.Trim()); command.Parameters.AddWithValue("goal", request.Goal.Trim()); command.Parameters.AddWithValue("status", request.Status.Trim());
        command.Parameters.AddWithValue("start", (object?)request.StartDate ?? DBNull.Value); command.Parameters.AddWithValue("end", (object?)request.EndDate ?? DBNull.Value);
    }

    private static async Task<string?> GetStatusAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string table, string id, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand($"select status from {table} where id = @id for update;", connection, transaction);
        command.Parameters.AddWithValue("id", id);
        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    private static async Task InsertReportItemsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string reportId, string itemType, IReadOnlyList<string>? items, CancellationToken cancellationToken)
    {
        if (items is null) return;
        const string sql = "insert into report_items (report_id, item_type, item_order, content) values (@report_id, @item_type, @item_order, @content);";
        for (var index = 0; index < items.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(items[index])) continue;
            await using var command = new NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("report_id", reportId);
            command.Parameters.AddWithValue("item_type", itemType);
            command.Parameters.AddWithValue("item_order", index + 1);
            command.Parameters.AddWithValue("content", items[index].Trim());
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task InsertAuditAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string action, string entityType, string entityId, string actor, string? previousValue, string? newValue, string? reason, CancellationToken cancellationToken)
    {
        const string sql = "insert into audit_logs (action, entity_type, entity_id, actor, previous_value, new_value, reason) values (@action, @entity_type, @entity_id, @actor, @previous_value, @new_value, @reason);";
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("action", action);
        command.Parameters.AddWithValue("entity_type", entityType);
        command.Parameters.AddWithValue("entity_id", entityId);
        command.Parameters.AddWithValue("actor", string.IsNullOrWhiteSpace(actor) ? "system" : actor.Trim());
        command.Parameters.AddWithValue("previous_value", (object?)previousValue ?? DBNull.Value);
        command.Parameters.AddWithValue("new_value", (object?)newValue ?? DBNull.Value);
        command.Parameters.AddWithValue("reason", (object?)reason ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
