using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task AddExpenseToHistory(IClientSessionHandle session, Group oldGroup, ObjectId OperationId, FilterDefinition<Group>? filter) {

    var oldExpense = oldGroup.Expenses.First(e => e.Id == OperationId);

    var snapShot = _mapper.Map<ExpenseSnapshot>(oldExpense);

    var update = Builders<Group>.Update.Push("Expenses.$.History", snapShot);

    await _groupCollection.FindOneAndUpdateAsync(session, filter, update);
  }
}