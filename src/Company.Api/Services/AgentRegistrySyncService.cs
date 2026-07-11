using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class AgentRegistrySyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly string _connectionString;
    private readonly string _definitionsPath;

    public AgentRegistrySyncService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");
        var configuredPath = configuration["AgentRegistry:DefinitionsPath"] ?? "../../../company-agents/agents";
        _definitionsPath = Path.GetFullPath(configuredPath, environment.ContentRootPath);
    }

    public async Task<int> SyncAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_definitionsPath))
            throw new DirectoryNotFoundException($"Agent definitions path not found: {_definitionsPath}");

        var synced = 0;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        foreach (var directory in Directory.EnumerateDirectories(_definitionsPath).OrderBy(Path.GetFileName))
        {
            var metadataPath = Path.Combine(directory, "tools.json");
            var metadata = JsonSerializer.Deserialize<AgentDefinitionMetadata>(await File.ReadAllTextAsync(metadataPath, cancellationToken), JsonOptions)
                ?? throw new InvalidDataException($"Invalid agent metadata: {metadataPath}");
            var id = ToAgentId(metadata.Agent);
            var prompt = await ReadRequiredAsync(directory, "prompt.md", cancellationToken);
            var rules = await ReadRequiredAsync(directory, "rules.md", cancellationToken);
            var workflow = await ReadRequiredAsync(directory, "workflow.md", cancellationToken);
            var memory = await ReadRequiredAsync(directory, "memory.md", cancellationToken);
            var toolsJson = JsonSerializer.Serialize(metadata.Tools ?? []);
            var updatedAt = Directory.EnumerateFiles(directory).Select(File.GetLastWriteTimeUtc).Max();

            const string sql = """
                insert into agents (id, name, role, department, status, current_task, prompt, rules, workflow, memory_scope, tools_json, definition_path, definition_updated_at_utc)
                values (@id, @name, @role, @department, 'Idle', null, @prompt, @rules, @workflow, @memory, @tools_json, @definition_path, @updated_at)
                on conflict (id) do update set
                    name = excluded.name,
                    role = excluded.role,
                    department = excluded.department,
                    prompt = excluded.prompt,
                    rules = excluded.rules,
                    workflow = excluded.workflow,
                    memory_scope = excluded.memory_scope,
                    tools_json = excluded.tools_json,
                    definition_path = excluded.definition_path,
                    definition_updated_at_utc = excluded.definition_updated_at_utc;
                """;
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("name", metadata.Title);
            command.Parameters.AddWithValue("role", metadata.Title);
            command.Parameters.AddWithValue("department", metadata.Department);
            command.Parameters.AddWithValue("prompt", prompt);
            command.Parameters.AddWithValue("rules", rules);
            command.Parameters.AddWithValue("workflow", workflow);
            command.Parameters.AddWithValue("memory", memory);
            command.Parameters.AddWithValue("tools_json", NpgsqlDbType.Jsonb, toolsJson);
            command.Parameters.AddWithValue("definition_path", directory);
            command.Parameters.AddWithValue("updated_at", new DateTimeOffset(updatedAt, TimeSpan.Zero));
            await command.ExecuteNonQueryAsync(cancellationToken);
            synced++;
        }
        return synced;
    }

    private static Task<string> ReadRequiredAsync(string directory, string fileName, CancellationToken cancellationToken) =>
        File.ReadAllTextAsync(Path.Combine(directory, fileName), cancellationToken);

    private static string ToAgentId(string name) => name switch
    {
        "CEOAssistant" => "ceo-assistant",
        _ => name.ToLowerInvariant()
    };

    private sealed record AgentDefinitionMetadata(string Agent, string Title, string Department, IReadOnlyList<string>? Tools);
}
