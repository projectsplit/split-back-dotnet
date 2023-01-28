using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CreateExpense(Expense newExpense, ObjectId groupId) {

    var update = Builders<Group>.Update.AddToSet("Expenses", newExpense);
    
    var group = await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, update);
    if(group is null) return Result.Failure($"Group with id {groupId} not found");
    
    return Result.Success();
  }
}