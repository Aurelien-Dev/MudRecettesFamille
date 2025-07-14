using System.Numerics.Tensors;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace RecettesFamille.Ai.Services;

// Custom interfaces to replace missing Microsoft.Extensions.VectorData types
public interface IVectorStore
{
    IVectorStoreRecordCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name) where TKey : notnull;
}

public interface IVectorStoreRecordCollection<TKey, TRecord> where TKey : notnull
{
    Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default);
    Task CreateCollectionAsync(CancellationToken cancellationToken = default);
    Task CreateCollectionIfNotExistsAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey key, CancellationToken cancellationToken = default);
    Task DeleteBatchAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default);
    Task DeleteCollectionAsync(CancellationToken cancellationToken = default);
    Task<TRecord?> GetAsync(TKey key, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TRecord> GetBatchAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default);
    Task<TKey> UpsertAsync(TRecord record, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TKey> UpsertBatchAsync(IEnumerable<TRecord> records, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    Task<VectorSearchResults<TRecord>> VectorizedSearchAsync(ReadOnlyMemory<float> vector, int top = 5, Func<TRecord, bool>? filter = null, CancellationToken cancellationToken = default);
}

public class VectorSearchResults<TRecord>
{
    public IAsyncEnumerable<VectorSearchResult<TRecord>> Results { get; }
    public VectorSearchResults(IAsyncEnumerable<VectorSearchResult<TRecord>> results) => Results = results;
}
public class VectorSearchResult<TRecord>
{
    public TRecord Record { get; }
    public float Similarity { get; }
    public VectorSearchResult(TRecord record, float similarity) { Record = record; Similarity = similarity; }
}

public class JsonVectorStore : IVectorStore
{
    private readonly string _basePath;
    public JsonVectorStore(string basePath) { _basePath = basePath; }
    public IVectorStoreRecordCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string name) where TKey : notnull
        => new JsonVectorStoreRecordCollection<TKey, TRecord>(name, Path.Combine(_basePath, name + ".json"));

    private class JsonVectorStoreRecordCollection<TKey, TRecord> : IVectorStoreRecordCollection<TKey, TRecord> where TKey : notnull
    {
        private static readonly Func<TRecord, TKey> _getKey = CreateKeyReader();
        private static readonly Func<TRecord, ReadOnlyMemory<float>> _getVector = CreateVectorReader();
        private readonly string _name;
        private readonly string _filePath;
        private Dictionary<TKey, TRecord>? _records;
        public JsonVectorStoreRecordCollection(string name, string filePath)
        {
            _name = name;
            _filePath = filePath;
            if (File.Exists(filePath))
                _records = JsonSerializer.Deserialize<Dictionary<TKey, TRecord>>(File.ReadAllText(filePath));
            else
                _records = new Dictionary<TKey, TRecord>();
        }
        public Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default) => Task.FromResult(_records is not null);
        public async Task CreateCollectionAsync(CancellationToken cancellationToken = default) { _records = new(); await WriteToDiskAsync(cancellationToken); }
        public async Task CreateCollectionIfNotExistsAsync(CancellationToken cancellationToken = default) { if (_records is null) await CreateCollectionAsync(cancellationToken); }
        public Task DeleteAsync(TKey key, CancellationToken cancellationToken = default) { _records!.Remove(key); return WriteToDiskAsync(cancellationToken); }
        public Task DeleteBatchAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default) { foreach (var key in keys) _records!.Remove(key); return WriteToDiskAsync(cancellationToken); }
        public Task DeleteCollectionAsync(CancellationToken cancellationToken = default) { _records = null; File.Delete(_filePath); return Task.CompletedTask; }
        public Task<TRecord?> GetAsync(TKey key, CancellationToken cancellationToken = default) => Task.FromResult(_records!.GetValueOrDefault(key));
        public async IAsyncEnumerable<TRecord> GetBatchAsync(IEnumerable<TKey> keys, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_records!.TryGetValue(key, out var record) && record is not null)
                {
                    yield return record;
                    await Task.Yield(); // pour que ce soit vraiment async si nécessaire
                }
            }
        }
        public async Task<TKey> UpsertAsync(TRecord record, CancellationToken cancellationToken = default) { var key = _getKey(record); _records![key] = record; await WriteToDiskAsync(cancellationToken); return key; }
        public async IAsyncEnumerable<TKey> UpsertBatchAsync(IEnumerable<TRecord> records, [EnumeratorCancellation] CancellationToken cancellationToken = default) { var results = new List<TKey>(); foreach (var record in records) { var key = _getKey(record); _records![key] = record; results.Add(key); } await WriteToDiskAsync(cancellationToken); foreach (var key in results) yield return key; }
        public Task<VectorSearchResults<TRecord>> VectorizedSearchAsync(
            ReadOnlyMemory<float> vector,
            int top = 5,
            Func<TRecord, bool>? filter = null,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<TRecord> filteredRecords = _records!.Values;
            if (filter is not null)
                filteredRecords = filteredRecords.Where(filter);

            var ranked = from record in filteredRecords
                         let candidateVector = _getVector(record)
                         let similarity = TensorPrimitives.CosineSimilarity(candidateVector.Span, vector.Span)
                         orderby similarity descending
                         select new VectorSearchResult<TRecord>(record, similarity);

            var topResults = ranked.Take(top);
            return Task.FromResult(new VectorSearchResults<TRecord>(ToAsync(topResults)));
        }

        private static async IAsyncEnumerable<VectorSearchResult<TRecord>> ToAsync(IEnumerable<VectorSearchResult<TRecord>> source)
        {
            foreach (var item in source)
            {
                yield return item;
                await Task.Yield(); // facultatif, juste pour que ce soit vraiment async
            }
        }
        private static Func<TRecord, TKey> CreateKeyReader()
        {
            var propertyInfo = typeof(TRecord).GetProperties().FirstOrDefault(p => p.Name == "Key" && p.PropertyType == typeof(TKey));
            if (propertyInfo == null) throw new InvalidOperationException("No property named 'Key' found on record type.");
            return record => (TKey)propertyInfo.GetValue(record)!;
        }
        private static Func<TRecord, ReadOnlyMemory<float>> CreateVectorReader()
        {
            var propertyInfo = typeof(TRecord).GetProperties().FirstOrDefault(p => p.Name == "Vector" && p.PropertyType == typeof(ReadOnlyMemory<float>));
            if (propertyInfo == null) throw new InvalidOperationException("No property named 'Vector' found on record type.");
            return record => (ReadOnlyMemory<float>)propertyInfo.GetValue(record)!;
        }
        private async Task WriteToDiskAsync(CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(_records);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
    }
}
