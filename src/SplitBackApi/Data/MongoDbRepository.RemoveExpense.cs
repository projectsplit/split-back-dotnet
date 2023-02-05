using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RemoveExpense(string groupId, string expenseId) {

    var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
    if(group is null) return Result.Failure($"Group {groupId} Not Found");

    var expenseToRemove = group.Expenses.Where(e => e.Id == expenseId).SingleOrDefault();
    if(expenseToRemove is null) return Result.Failure($"Expense with id {expenseId} not found");

    group.Expenses.Remove(expenseToRemove);
    group.DeletedExpenses.Add(expenseToRemove);

    await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);

    return Result.Success();
  }
}