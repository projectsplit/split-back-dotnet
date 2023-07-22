using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExpenseRepository;

public class ExpenseMongoDbRepository : IExpenseRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Expense> _expenseCollection;
  private readonly IMongoCollection<PastExpense> _pastExpenseCollection;

  public ExpenseMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _expenseCollection = mongoDatabase.GetCollection<Expense>(dbSettings.Database.Collections.Expenses);
    _pastExpenseCollection = mongoDatabase.GetCollection<PastExpense>(dbSettings.Database.Collections.PastExpenses);
  }

  public async Task Create(Expense expense) {
    expense.Id = ObjectId.GenerateNewId().ToString();

    await _expenseCollection.InsertOneAsync(expense);
  }

  public async Task<Result> DeleteById(string expenseId) {

    var findExpenseFilter = Builders<Expense>.Filter.Eq(e => e.Id, expenseId);

    var expenseFound = await _expenseCollection.Find<Expense>(findExpenseFilter).FirstOrDefaultAsync();
    if(expenseFound is null) return Result.Failure($"Expense with id {expenseId} has not been found to be deleted");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await _expenseCollection.DeleteOneAsync(session, findExpenseFilter);

      await _pastExpenseCollection.InsertOneAsync(session, expenseFound.ToPastExpense());

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<Result<Expense>> GetPastExpenseById(string expenseId) {

    var filter = Builders<Expense>.Filter.Eq(e => e.Id, expenseId);

    var expense = await _expenseCollection.Find(filter).FirstOrDefaultAsync();
    if(expense is null) return Result.Failure<Expense>($"Expense with id {expenseId} not found");

    return expense;
  }

  public async Task<Result<Expense>> GetById(string expenseId) {

    var filter = Builders<Expense>.Filter.Eq(e => e.Id, expenseId);

    var expense = await _expenseCollection.Find(filter).FirstOrDefaultAsync();
    if(expense is null) return Result.Failure<Expense>($"Expense with id {expenseId} not found");

    return expense;
  }

  public async Task<List<Expense>> GetByGroupId(string groupId) {

    var filter = Builders<Expense>.Filter.Eq(e => e.GroupId, groupId);

    var expenses = await _expenseCollection.Find(filter).ToListAsync();

    return expenses;
  }

  public async Task<List<Expense>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize) {

    var filter = Builders<Expense>.Filter.Eq(e => e.GroupId, groupId);
    var query = _expenseCollection.Find(filter);

    int skipCount = (pageNumber - 1) * pageSize;
    query = query.Limit(pageSize);
    query = query.Skip(skipCount);

    return await query.ToListAsync();
  }

  public async Task<Result> Update(Expense editedExpense) {

    var findExpenseFilter = Builders<Expense>.Filter.Eq(e => e.Id, editedExpense.Id);

    var currentExpense = await _expenseCollection.Find(findExpenseFilter).FirstOrDefaultAsync();
    if(currentExpense is null) return Result.Failure($"Expense with id {editedExpense.Id} has not been found");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {
      await _expenseCollection.ReplaceOneAsync(session, findExpenseFilter, editedExpense);

      await _pastExpenseCollection.InsertOneAsync(session, currentExpense.ToPastExpense());

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();

      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<List<Expense>> GetLatest(string groupId, int limit, DateTime lastDateTime) {
    var filterCreationTime = Builders<Expense>.Filter.Lt(u => u.CreationTime, lastDateTime);
    var filterGroupId = Builders<Expense>.Filter.Eq(e => e.GroupId, groupId);

    var combinedFilter = filterCreationTime & filterGroupId;

    var sort = Builders<Expense>.Sort.Descending(u => u.CreationTime);

    return await _expenseCollection.Find(combinedFilter).Sort(sort).Limit(limit).ToListAsync();
  }
}