
using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain.Models;
using Microsoft.Extensions.Options;

using SplitBackApi.Configuration;

namespace SplitBackApi.Data.Repositories.GroupFiltersRepository;

public class GroupFiltersMongoDbRepository : IGroupFiltersRepository
{

    private readonly IMongoCollection<GroupFilter> _groupFiltersCollection;

    public GroupFiltersMongoDbRepository(
        IOptions<AppSettings> appSettings)
    {
        var dbSettings = appSettings.Value.MongoDb;
        var mongoClient = new MongoClient(dbSettings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Database.Name);

        _groupFiltersCollection = mongoDatabase.GetCollection<GroupFilter>(dbSettings.Database.Collections.GroupFilters);

    }
    public async Task<Result> Create(GroupFilter groupFilter)
    {
        groupFilter.Id = ObjectId.GenerateNewId().ToString();

        await _groupFiltersCollection.InsertOneAsync(groupFilter);
    }

    public Task<Result<GroupFilter>> GetByGroupId(string groupId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Update(GroupFilter groupFilter)
    {
        var filter = Builders<GroupFilter>.Filter.Eq(g => g.Id, groupFilter.Id);

        var replaceResult = await _groupFiltersCollection.ReplaceOneAsync(filter, groupFilter);

        return replaceResult.IsAcknowledged ? Result.Success() : Result.Failure("Failed to update filter");
    }

}

