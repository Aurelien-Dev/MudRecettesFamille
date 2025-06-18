using Microsoft.SemanticKernel.Memory;
using System.Runtime.CompilerServices;
using System.Text.Json;

public class JsonMemoryStore(string basePath) : IMemoryStore
{
    private readonly Dictionary<string, Dictionary<string, MemoryRecord>> _collections = [];

    public Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collection))
        {
            _collections[collection] = [];
        }
        return SaveCollectionAsync(collection, cancellationToken);
    }

    public Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        _collections.Remove(collection);
        File.Delete(GetFilePath(collection));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_collections.Keys.AsEnumerable());
    }

    public Task<MemoryRecord?> GetAsync(string collection, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collection, out var records) && records.TryGetValue(key, out var record))
        {
            return Task.FromResult<MemoryRecord?>(record);
        }

        return Task.FromResult<MemoryRecord?>(null);
    }

    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collection, IEnumerable<string> keys, bool withEmbedding = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collection, out var records))
        {
            foreach (var key in keys)
            {
                if (records.TryGetValue(key, out var record))
                {
                    yield return record;
                }
            }
        }
        await Task.CompletedTask;
    }

    public Task RemoveAsync(string collection, string key, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collection, out var records))
        {
            records.Remove(key);
        }
        return SaveCollectionAsync(collection, cancellationToken);
    }

    public async IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collection, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore, bool withEmbedding = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_collections.TryGetValue(collection, out var records))
            yield break;

        var results = records.Values
            .Select(r => (Record: r, Score: CosineSimilarity(r.Embedding?.ToArray(), embedding.ToArray())))
            .Where(x => x.Score >= minRelevanceScore)
            .OrderByDescending(x => x.Score)
            .Take(limit);

        foreach (var (record, score) in results)
        {
            yield return (record, score);
        }

        await Task.CompletedTask;
    }

    public async Task UpsertAsync(string collection, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        if (!_collections.TryGetValue(collection, out var records))
        {
            records = [];
            _collections[collection] = records;
        }

        records[record.Id] = record;
        await SaveCollectionAsync(collection, cancellationToken);
    }

    private static double CosineSimilarity(float[]? v1, float[]? v2)
    {
        if (v1 is null || v2 is null || v1.Length != v2.Length)
            return 0;

        double dot = 0, mag1 = 0, mag2 = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8); // stabilité numérique
    }

    private string GetFilePath(string collection) => Path.Combine(basePath, $"{collection}.json");

    private async Task SaveCollectionAsync(string collection, CancellationToken cancellationToken)
    {
        var path = GetFilePath(collection);
        var content = JsonSerializer.Serialize(_collections[collection]);
        Directory.CreateDirectory(basePath);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    // (Optionnel) pour charger les collections existantes au démarrage
    public void LoadAll()
    {
        foreach (var file in Directory.EnumerateFiles(basePath, "*.json"))
        {
            var collectionName = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var dict = JsonSerializer.Deserialize<Dictionary<string, MemoryRecord>>(json);
            if (dict != null)
                _collections[collectionName] = dict;
        }
    }
}
