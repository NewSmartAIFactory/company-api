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
