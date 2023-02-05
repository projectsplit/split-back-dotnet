using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> GetGroupIfUserIsNotMember(string userId, string groupId) {

    var filter =
      Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) &
      Builders<Group>.Filter.Ne("Members", userId.ToObjectId());

    var group = await _groupCollection.Find(filter).SingleOrDefaultAsync();

    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");

    return group;
  }
}