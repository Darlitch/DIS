using Contract.Api.Enums;
using Manager.Models;
using MongoDB.Driver;

namespace Manager.Repositories;

public class SubtaskRepository(IMongoDatabase database)
{
    private readonly IMongoCollection<SubtaskDocument> _subtasks =
        database.GetCollection<SubtaskDocument>("subtasks").WithWriteConcern(WriteConcern.WMajority);
    
    public async Task CreateIndexesAsync(CancellationToken ct = default)
    {
        var index = new CreateIndexModel<SubtaskDocument>(
            Builders<SubtaskDocument>.IndexKeys
                .Ascending(x => x.RequestId)
                .Ascending(x => x.PartNumber),
            new CreateIndexOptions { Unique = true });
        await _subtasks.Indexes.CreateOneAsync(index, cancellationToken: ct);
    }
    
    public Task CreateManyAsync(IClientSessionHandle session, RequestDocument request, CancellationToken ct = default)
    {
        var subtasks = Enumerable.Range(0, request.PartCount)
            .Select(i => new SubtaskDocument
            {
                RequestId = request.RequestId,
                Hash = request.Hash,
                MaxLength = request.MaxLength,
                PartCount = request.PartCount,
                PartNumber = i
            })
            .ToList();

        return _subtasks.InsertManyAsync(session, subtasks, cancellationToken: ct);
    }

    public Task ResetStaleQueuedToPendingAsync(TimeSpan staleAfter, CancellationToken ct = default)
    {
        var update = Builders<SubtaskDocument>.Update
            .Set(x => x.Status, SubtaskStatus.PENDING_DISPATCH)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        return _subtasks.UpdateManyAsync(x =>
            x.Status == SubtaskStatus.QUEUED && x.UpdatedAt < DateTime.UtcNow - staleAfter, update, cancellationToken: ct);
    }
    
    public async Task<long> CountNotCompletedAsync(Guid requestId, CancellationToken ct = default)
    {
        return await _subtasks.CountDocumentsAsync(x =>x.RequestId == requestId && x.Status != SubtaskStatus.COMPLETED, cancellationToken: ct);
    }
    
    public async Task<SubtaskDocument?> GetAsync(Guid requestId, int partNumber, CancellationToken ct = default)
    {
        return await _subtasks.Find(x => x.RequestId == requestId && x.PartNumber == partNumber).FirstOrDefaultAsync(ct);
    }
    
    public async Task<List<SubtaskDocument>> GetPendingDispatchAsync(CancellationToken ct = default)
    {
        return await _subtasks
            .Find(x => x.Status == SubtaskStatus.PENDING_DISPATCH)
            .SortBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public Task UpdateAnswersAsync(Guid requestId, int partNumber, List<string> answers, CancellationToken ct = default)
    {
        var update = Builders<SubtaskDocument>.Update
            .Set(x => x.Answers, answers)
            .Set(x => x.Status, SubtaskStatus.COMPLETED)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        return _subtasks.UpdateOneAsync(x => x.RequestId == requestId && x.PartNumber == partNumber, update, cancellationToken: ct);
    }
    
    public Task UpdateStatusAsync(Guid requestId, int partNumber, SubtaskStatus status, CancellationToken ct = default)
    {
        var update = Builders<SubtaskDocument>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        return _subtasks.UpdateOneAsync(x => x.RequestId == requestId && x.PartNumber == partNumber, update, cancellationToken: ct);
    }
}