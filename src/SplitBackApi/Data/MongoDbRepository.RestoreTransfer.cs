using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RestoreTransfer(string groupId, string transferId) {
    
    var groupBsonId = ObjectId.Parse(groupId);
    var transferBsonId = ObjectId.Parse(transferId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var transferToRestore = group.DeletedTransfers.Where(t => t.Id == transferBsonId).SingleOrDefault();
      if(transferToRestore is null) return Result.Failure($"Transfer with id {transferId} not found");

      group.DeletedTransfers.Remove(transferToRestore);
      group.Transfers.Add(transferToRestore);
      
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
      await session.CommitTransactionAsync();

    } catch(Exception _) {
      
      await session.AbortTransactionAsync();
    }
    
    return Result.Success();
  }
}