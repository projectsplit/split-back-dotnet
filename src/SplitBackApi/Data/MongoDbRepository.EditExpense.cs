using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId) {

    //var expenseId = ObjectId.Parse("63ac1e064b49cf6ddbf27738");
    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
    var updateExpense = Builders<Group>.Update
      .Set("Expenses.$.Description", newExpense.Description)
      .Set("Expenses.$.Amount", newExpense.Amount)
      .Set("Expenses.$.Spenders", newExpense.Spenders)
      .Set("Expenses.$.Participants", newExpense.Participants)
      //.Set("Expenses.$.Label", newExpense.Labels)
      .Set("Expenses.$.IsoCode", newExpense.IsoCode);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {

      var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
      if(oldGroup is null) return Result.Failure("Group not found");

      await AddExpenseToHistory(oldGroup, expenseId, filter);

    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();
  }
}