using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RestoreExpense(string groupId, string expenseId) {

    var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
    if(group is null) return Result.Failure($"Group {groupId} Not Found");

    var expenseToRestore = group.DeletedExpenses.Where(e => e.Id == expenseId).SingleOrDefault();
    if(expenseToRestore is null) return Result.Failure($"Expense with id {expenseId} not found");

    group.DeletedExpenses.Remove(expenseToRestore);
    group.Expenses.Add(expenseToRestore);

    await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);
    
    return Result.Success();
  }
}