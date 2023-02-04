using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  public async Task<Result> EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId) {

    //var transferId = ObjectId.Parse("63aafa3ad36b483e99735bcd");
    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Transfers, t => t.Id == transferId);

    var updateTransfer = Builders<Group>.Update
       .Set("Transfers.$.Description", newTransfer.Description)
       .Set("Transfers.$.Amount", newTransfer.Amount)
       .Set("Transfers.$.SenderId", newTransfer.SenderId)
       .Set("Transfers.$.ReceiverId", newTransfer.ReceiverId)
       .Set("Transfers.$.IsoCode", newTransfer.IsoCode);

    using var session = await _mongoClient.StartSessionAsync();

    session.StartTransaction();

    try {

      var oldGroup = await _groupCollection.FindOneAndUpdateAsync(session, filter, updateTransfer);
      if(oldGroup is null) return Result.Failure("Group not found");
      
      await AddTransferToHistory(session, oldGroup, transferId, filter);
      session.CommitTransaction();

    } catch(Exception ex) {

      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }

    return Result.Success();
  }
}