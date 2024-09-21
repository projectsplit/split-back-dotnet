
using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SplitBackApi.Domain.Models;
using Microsoft.Extensions.Options;
using SplitBackApi.Common;
using SplitBackApi.Configuration;


namespace SplitBackApi.Data.Repositories.GroupFiltersRepository;

public class GroupFiltersMongoDbRepository : IGroupFiltersRepository
{

  private readonly IMongoCollection<GroupFilter> _groupFiltersCollection;
  private readonly MongoClient _mongoClient;

  public GroupFiltersMongoDbRepository(IOptions<AppSettings> appSettings)
  {
    var dbSettings = appSettings.Value.MongoDb;
    var _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _groupFiltersCollection = mongoDatabase.GetCollection<GroupFilter>(dbSettings.Database.Collections.GroupFilters);

  }

  public async Task<Result> Create(GroupFilter groupFilter, CancellationToken ct)
  {
    using var session = await _mongoClient.StartSessionAsync(cancellationToken: ct);
    session.StartTransaction();

    try
    {
      await DeleteByGroupId(groupFilter.GroupId);
      var insertOptions = new InsertOneOptions();
      await _groupFiltersCollection.InsertOneAsync(groupFilter, insertOptions, ct).ExecuteResultAsync();
    }
    catch (MongoException e)
    {

      await session.AbortTransactionAsync(ct);
      return Result.Failure(e.ToString());
    }

    return Result.Success();

  }
  public async Task<Result> DeleteByGroupId(string groupId) //Task Result
  {
    var findGroupFilter = Builders<GroupFilter>.Filter.Eq(gf => gf.GroupId, groupId);

    await _groupFiltersCollection.DeleteOneAsync(findGroupFilter);

    return Result.Success();
  }

  public async Task<Result<GroupFilter>> GetByGroupId(string groupId)
  {
    var filter = Builders<GroupFilter>.Filter.Eq(gf => gf.GroupId, groupId);

    return await _groupFiltersCollection.Find(filter).FirstOrDefaultAsync();
  }
}

