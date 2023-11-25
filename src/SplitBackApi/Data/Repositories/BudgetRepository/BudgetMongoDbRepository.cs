using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Common;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.BudgetRepository;

public class BudgetMongoDbRepository : IBudgetRepository
{
  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Budget> _budgetCollection;
  private readonly IMongoCollection<Expense> _expenseCollection;

  public BudgetMongoDbRepository(
    IOptions<AppSettings> appSettings
  )
  {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _budgetCollection = mongoDatabase.GetCollection<Budget>(dbSettings.Database.Collections.Budgets);
    _expenseCollection = mongoDatabase.GetCollection<Expense>(dbSettings.Database.Collections.Expenses);

  }
  public async Task<Result> Create(Budget budget, string userId, CancellationToken ct)
  {
    using var session = await _mongoClient.StartSessionAsync(cancellationToken: ct);
    session.StartTransaction();

    try
    {
      await DeleteByUserId(userId);
      await _budgetCollection.InsertOneAsync(budget, ct).ExecuteResultAsync();
    }
    catch (MongoException e)
    {

      await session.AbortTransactionAsync(ct);
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<Maybe<DeleteResult>> DeleteByUserId(string UserId) //Task Result
  {
    var findBudgetFilter = Builders<Budget>.Filter.Eq(b => b.UserId, UserId);

    // var budgetFound = await _budgetCollection.Find(findBudgetFilter).FirstOrDefaultAsync();
    // if (budgetFound is null) return Result.Failure($"Budget from user {UserId} has not been found to be deleted");

    return await _budgetCollection.DeleteOneAsync(findBudgetFilter);

  }

  public async Task<Maybe<Budget>> GetByUserId(string userId)
  {
    var filter = Builders<Budget>.Filter.Eq(b => b.UserId, userId);

    return await _budgetCollection.Find(filter).FirstOrDefaultAsync();
  }

  // public Task<Result<Expense>> GetExpensesByBudgetType(BudgetType BudgetType, string day)
  // {
  //   DateTime now = DateTime.Now;
  //   DateTime startDate;

  //   var filterCreationTime = Builders<Expense>.Filter.Lt(u => u.CreationTime, lastDateTime);
  // }
}