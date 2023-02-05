using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RestoreTransfer(string groupId, string transferId) {

    var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
    if(group is null) return Result.Failure($"Group {groupId} Not Found");

    var transferToRestore = group.DeletedTransfers.Where(t => t.Id == transferId).SingleOrDefault();
    if(transferToRestore is null) return Result.Failure($"Transfer with id {transferId} not found");

    group.DeletedTransfers.Remove(transferToRestore);
    group.Transfers.Add(transferToRestore);

    await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);

    return Result.Success();
  }
}