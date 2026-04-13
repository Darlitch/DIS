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
    
    public async Task<SubtaskDocument> CreateAsync(RequestDocument request, int partNumber,
        CancellationToken ct = default)
    {
        var doc = new SubtaskDocument
        {
            RequestId = request.RequestId,
            Hash = request.Hash,
            MaxLength = request.MaxLength,
            PartCount = request.PartCount,
            PartNumber = partNumber
        };
        await _subtasks.InsertOneAsync(doc, cancellationToken: ct);
        return doc;
    }
    
    public Task<long> CountNotCompletedAsync(Guid requestId, CancellationToken ct = default)
    {
        return _subtasks.CountDocumentsAsync(x =>x.RequestId == requestId && x.Status != SubtaskStatus.COMPLETED, cancellationToken: ct);
    }
    
    public Task<SubtaskDocument?> GetAsync(Guid requestId, int partNumber, CancellationToken ct = default)
    {
        return _subtasks.Find(x => x.RequestId == requestId && x.PartNumber == partNumber).FirstOrDefaultAsync(ct);
    }

    public Task UpdateAnswersAsync(Guid requestId, int partNumber, List<string> answers, CancellationToken ct = default)
    {
        var update = Builders<SubtaskDocument>.Update
            .Set(x => x.Answers, answers)
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