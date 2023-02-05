using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> GetGroupById(string groupId) {

    var group = await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId.ToObjectId())).FirstOrDefaultAsync();
    if(group is null) return Result.Failure<Group>($"Group with id {groupId} not found");

    return group;
  }
}