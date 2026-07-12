using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NewSmartAIFactory.CompanyApi.Models;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class QdrantMemoryIndexService
{
    private const string Collection = "company_memory";
    private const int VectorSize = 64;
    private readonly HttpClient _http;
    private readonly MemoryService _memory;

    public QdrantMemoryIndexService(IHttpClientFactory factory, IConfiguration configuration, MemoryService memory)
    {
        _http = factory.CreateClient("qdrant");
        _http.BaseAddress = new Uri(configuration["Infrastructure:Qdrant"] ?? "http://localhost:6333");
        _memory = memory;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync($"collections/{Collection}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync($"collections/{Collection}", cancellationToken);
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode != HttpStatusCode.NotFound) response.EnsureSuccessStatusCode();

        using var create = await _http.PutAsJsonAsync($"collections/{Collection}", new
        {
            vectors = new { size = VectorSize, distance = "Cosine" }
        }, cancellationToken);
        create.EnsureSuccessStatusCode();
    }

    public async Task<MemorySummary?> IndexAsync(Guid id, CancellationToken cancellationToken)
    {
        var memory = await _memory.GetAsync(id, cancellationToken);
        if (memory is null) return null;
        await EnsureCollectionAsync(cancellationToken);

        var text = $"{memory.Title}\n{memory.Content}\n{memory.Scope}\n{memory.MemoryType}";
        var vector = BuildVector(text);
        using var response = await _http.PutAsJsonAsync($"collections/{Collection}/points?wait=true", new
        {
            points = new[] { new
            {
                id = memory.Id,
                vector,
                payload = new { memoryId = memory.Id, memory.Scope, memory.MemoryType, memory.Title, memory.Content, memory.ProjectId, memory.AgentId, memory.Source }
            }}
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return memory;
    }

    public async Task<IReadOnlyList<MemorySummary>> SearchAsync(string query, string? scope, int limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<MemorySummary>();
        await EnsureCollectionAsync(cancellationToken);
        using var response = await _http.PostAsJsonAsync($"collections/{Collection}/points/search", new
        {
            vector = BuildVector(query),
            limit = Math.Clamp(limit, 1, 50),
            with_payload = false,
            filter = string.IsNullOrWhiteSpace(scope) ? null : new { must = new[] { new { key = "scope", match = new { value = scope } } } }
        }, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var ids = document.RootElement.GetProperty("result")
            .EnumerateArray()
            .Select(x => Guid.TryParse(x.GetProperty("id").GetString(), out var id) ? id : (Guid?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();
        var results = new List<MemorySummary>();
        foreach (var id in ids)
        {
            var memory = await _memory.GetAsync(id, cancellationToken);
            if (memory is { IsObsolete: false }) results.Add(memory);
        }
        return results;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsJsonAsync($"collections/{Collection}/points/delete?wait=true", new
        {
            points = new[] { id }
        }, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.EnsureSuccessStatusCode();
    }

    private static double[] BuildVector(string text)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        var vector = new double[VectorSize];
        for (var i = 0; i < vector.Length; i++)
        {
            var b = digest[i % digest.Length];
            vector[i] = (b / 127.5d) - 1d;
        }
        var norm = Math.Sqrt(vector.Sum(x => x * x));
        return norm == 0 ? vector : vector.Select(x => x / norm).ToArray();
    }
}
