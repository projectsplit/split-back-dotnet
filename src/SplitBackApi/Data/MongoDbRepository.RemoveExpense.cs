using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RemoveExpense(string groupId, string expenseId) {

    var groupBsonId = ObjectId.Parse(groupId);
    var expenseBsonId = ObjectId.Parse(expenseId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var expenseToRemove = group.Expenses.Where(e => e.Id == expenseBsonId).SingleOrDefault();
      if(expenseToRemove is null) return Result.Failure($"Expense with id {expenseId} not found");
      
      group.Expenses.Remove(expenseToRemove);
      group.DeletedExpenses.Add(expenseToRemove);
      
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);

      await session.CommitTransactionAsync();
      
    } catch(Exception) {
      
      await session.AbortTransactionAsync();
    }
    
    return Result.Success();
  }
}