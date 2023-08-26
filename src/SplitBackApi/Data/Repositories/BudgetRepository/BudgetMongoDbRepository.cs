using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
  public async Task Create(Budget budget)
  {
    await _budgetCollection.InsertOneAsync(budget);
  }

  public async Task<Result> DeleteByUserId(string UserId)
  {
    var findBudgetFilter = Builders<Budget>.Filter.Eq(b => b.UserId, UserId);

    var budgetFound = await _budgetCollection.Find(findBudgetFilter).FirstOrDefaultAsync();
    if (budgetFound is null) return Result.Failure($"Budget from user {UserId} has not been found to be deleted");

    await _budgetCollection.DeleteOneAsync(findBudgetFilter);
    return Result.Success();

  }

  public async Task<Result<Budget>> GetByUserId(string UserId)
  {
    var filter = Builders<Budget>.Filter.Eq(b => b.UserId, UserId);

    var budget = await _budgetCollection.Find(filter).FirstOrDefaultAsync();
    if (budget is null) return Result.Failure<Budget>($"budget from {UserId} not found");

    return budget;
  }

  // public Task<Result<Expense>> GetExpensesByBudgetType(BudgetType BudgetType, string day)
  // {
  //   DateTime now = DateTime.Now;
  //   DateTime startDate;

  //   var filterCreationTime = Builders<Expense>.Filter.Lt(u => u.CreationTime, lastDateTime);
  // }
}