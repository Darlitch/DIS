using Contract.Api.Enums;
using Manager.Models;
using MongoDB.Driver;

namespace Manager.Repositories;

public class RequestRepository(IMongoDatabase database)
{
    private readonly IMongoCollection<RequestDocument> _requests =
        database.GetCollection<RequestDocument>("requests").WithWriteConcern(WriteConcern.WMajority);

    public async Task CreateIndexesAsync(CancellationToken ct = default)
    {
        var index = new CreateIndexModel<RequestDocument>(
            Builders<RequestDocument>.IndexKeys.Ascending(x => x.RequestId),
            new CreateIndexOptions { Unique = true });
        await _requests.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }

    public async Task<RequestDocument> CreateAsync(string hash, int maxLength, int partCount,
        CancellationToken ct = default)
    {
        var doc = new RequestDocument
        {
            RequestId = Guid.NewGuid(),
            Hash = hash,
            MaxLength = maxLength,
            PartCount = partCount
        };
        await _requests.InsertOneAsync(doc, cancellationToken: ct);
        return doc;
    }
    
    public Task<long> CountInProgressAsync(CancellationToken ct = default)
    {
        return _requests.CountDocumentsAsync(x => x.Status == RequestStatus.IN_PROGRESS, cancellationToken: ct);
    }
    
    public Task<RequestDocument?> GetAsync(Guid requestId, CancellationToken ct = default)
    {
        return _requests.Find(x => x.RequestId == requestId).FirstOrDefaultAsync(ct);
    }
    
    public Task<RequestDocument?> GetAsync(string hash, int maxLength, CancellationToken ct = default)
    {
        return _requests.Find(x => x.Hash == hash && x.MaxLength == maxLength).FirstOrDefaultAsync(ct);
    }
    
    public Task UpdateProgressAsync(Guid requestId, List<string> answers, int completedParts, RequestStatus requestStatus, CancellationToken ct)
    {
        var update = Builders<RequestDocument>.Update
            .Set(x => x.Answers, answers)
            .Set(x => x.CompletedParts, completedParts)
            .Set(x => x.Status, requestStatus)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        return _requests.UpdateOneAsync(x => x.RequestId == requestId, update, cancellationToken: ct);
    }

}