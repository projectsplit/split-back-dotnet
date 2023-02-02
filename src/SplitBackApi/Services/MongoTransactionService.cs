using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;

namespace SplitBackApi.Services;

public class MongoTransactionService {
  private readonly string _connectionString;

  public MongoTransactionService(IOptions<AppSettings> appSettings) {
    var dbSettings = appSettings.Value.MongoDb;
    var mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(dbSettings.Database.Name);
    _connectionString = dbSettings.ConnectionString;
  }

  public async Task<Result> RunMongoTransaction(Func<Task<Result>> code, MongoClient mongoClient) {

    using var session = await mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      var result = await code();
      await session.CommitTransactionAsync();
      return result;

    } catch(Exception) {

      await session.AbortTransactionAsync();
      return Result.Failure("Transaction failed");
      
    }
  }
}