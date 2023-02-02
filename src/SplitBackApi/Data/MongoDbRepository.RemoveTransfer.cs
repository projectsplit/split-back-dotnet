using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result> RemoveTransfer(string groupId, string transferId) {
    
    var groupBsonId = ObjectId.Parse(groupId);
    var transferBsonId = ObjectId.Parse(transferId);

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {
      
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var transferToRemove = group.Transfers.Where(t => t.Id == transferBsonId).SingleOrDefault();
      if(transferToRemove is null) return Result.Failure($"Transfer with id {transferId} not found");
      
      group.Transfers.Remove(transferToRemove);
      group.DeletedTransfers.Add(transferToRemove);
      
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);

      await session.CommitTransactionAsync();
      
    } catch(Exception _) {
      
      await session.AbortTransactionAsync();
    }
    
    return Result.Success();
  }
}