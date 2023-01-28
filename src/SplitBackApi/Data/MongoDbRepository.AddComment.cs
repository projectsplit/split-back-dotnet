using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result> AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId) {

    var filter = 
      Builders<Group>.Filter.Eq("_id", groupId) & 
      Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
      
    var groupUpdate = Builders<Group>.Update.Push("Expenses.$.Comments", newComment);
    
    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"expense {expenseId} in group {groupId} not found");
    
    return Result.Success();
  }
}