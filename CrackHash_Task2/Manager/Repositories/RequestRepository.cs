using Contract.Api.Enums;
using Manager.Models;
using MongoDB.Driver;

namespace Manager.Repositories;

public class RequestRepository(IMongoDatabase database)
{
    private readonly IMongoCollection<RequestDocument> _requests =
        database.GetCollection<RequestDocument>("requests").WithWriteConcern(WriteConcern.WMajority);

    public Task CreateIndexesAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public async Task<RequestDocument> CreateAsync(IClientSessionHandle session, string hash,
        int maxLength, int partCount, CancellationToken ct = default)
    {
        var doc = new RequestDocument
        {
            RequestId = Guid.NewGuid(),
            Hash = hash,
            MaxLength = maxLength,
            PartCount = partCount
        };

        await _requests.InsertOneAsync(session, doc, cancellationToken: ct);
        return doc;
    }
    
    public Task<long> CountInProgressAsync(CancellationToken ct = default)
    {
        return _requests.CountDocumentsAsync(x => x.Status == RequestStatus.IN_PROGRESS, cancellationToken: ct);
    }
    
    public async Task<RequestDocument?> GetAsync(Guid requestId, CancellationToken ct = default)
    {
        return await _requests.Find(x => x.RequestId == requestId).FirstOrDefaultAsync(ct);
    }
    
    public async Task<RequestDocument?> GetAsync(string hash, int maxLength, CancellationToken ct = default)
    {
        return await _requests.Find(x => x.Hash == hash && x.MaxLength == maxLength).FirstOrDefaultAsync(ct);
    }

    public Task AddAnswersAsync(Guid requestId, List<string> answers, CancellationToken ct = default)
    {
        var update = Builders<RequestDocument>.Update
            .AddToSetEach(x => x.Answers, answers)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        return _requests.UpdateOneAsync(x => x.RequestId == requestId, update, cancellationToken: ct);
    }

    public Task UpdateStatusAsync(Guid requestId, RequestStatus requestStatus, CancellationToken ct = default)
    {
        var update = Builders<RequestDocument>.Update
            .Set(x => x.Status, requestStatus)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        return _requests.UpdateOneAsync(x => x.RequestId == requestId, update, cancellationToken: ct);
    }

}