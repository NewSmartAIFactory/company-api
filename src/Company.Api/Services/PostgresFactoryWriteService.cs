using Npgsql;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class PostgresFactoryWriteService
{
    private static readonly HashSet<string> AllowedTaskStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Todo",
        "Doing",
        "Done",
        "Blocked"
    };

    private readonly string _connectionString;

    public PostgresFactoryWriteService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");
    }

    public async Task<bool> UpdateTaskStatusAsync(string id, string status, CancellationToken cancellationToken)
    {
        if (!AllowedTaskStatuses.Contains(status))
        {
            throw new ArgumentException($"Unsupported task status: {status}", nameof(status));
        }

        const string sql = """
            update tasks
            set status = @status
            where id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public Task<bool> ApproveDecisionAsync(string id, CancellationToken cancellationToken)
    {
        return SetDecisionStatusAsync(id, "Approved", cancellationToken);
    }

    public Task<bool> RejectDecisionAsync(string id, CancellationToken cancellationToken)
    {
        return SetDecisionStatusAsync(id, "Rejected", cancellationToken);
    }

    private async Task<bool> SetDecisionStatusAsync(string id, string status, CancellationToken cancellationToken)
    {
        const string sql = """
            update decisions
            set status = @status
            where id = @id;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }
}