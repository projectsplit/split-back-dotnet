using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.GroupRepository;

public class GroupMongoDbRepository : IGroupRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Group> _groupCollection;

  public GroupMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _groupCollection = mongoDatabase.GetCollection<Group>(dbSettings.Database.Collections.Groups);
  }

  public async Task Create(Group group) {
    group.Id = ObjectId.GenerateNewId().ToString();

    await _groupCollection.InsertOneAsync(group);
  }

  public Task<Result> DeleteById(string groupId) {
    throw new NotImplementedException();
  }

  public async Task<Maybe<Group>> GetById(string groupId) {

    var filter = Builders<Group>.Filter.Eq(g => g.Id, groupId);

    return await _groupCollection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Result> Update(Group group) {

    var filter = Builders<Group>.Filter.Eq(g => g.Id, group.Id);

    var replaceResult = await _groupCollection.ReplaceOneAsync(filter, group);

    if(replaceResult.IsAcknowledged) return Result.Success();

    return Result.Failure("Failed to update group");
  }

  public async Task<Result<List<Group>>> GetGroupsByUserId(string userId) {

    var filter = Builders<Group>.Filter.ElemMatch(g => g.Members, m => m is UserMember && ((UserMember)m).UserId == userId);
    var groups = await _groupCollection.Find(filter).ToListAsync();

    return groups;
  }

}