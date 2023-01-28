using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result> RestoreExpense(string groupId, string expenseId) {

    var groupBsonId = ObjectId.Parse(groupId);
    var expenseBsonId = ObjectId.Parse(expenseId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var expenseToRestore = group.DeletedExpenses.Where(e => e.Id == expenseBsonId).SingleOrDefault();
      if(expenseToRestore is null) return Result.Failure($"Expense with id {expenseId} not found");
      
      group.DeletedExpenses.Remove(expenseToRestore);
      group.Expenses.Add(expenseToRestore);
      
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
      
      await session.CommitTransactionAsync();

    } catch(Exception _) {
      
      await session.AbortTransactionAsync();
    }
    
    return Result.Success();
  }
}