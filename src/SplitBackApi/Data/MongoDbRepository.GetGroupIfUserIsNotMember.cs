using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> GetGroupIfUserIsNotMember(ObjectId userId, ObjectId groupId) {

    var filter =
      Builders<Group>.Filter.Eq("_id", groupId) &
      Builders<Group>.Filter.Ne("Members", userId);

    var group = await _groupCollection.Find(filter).SingleOrDefaultAsync();

    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");

    return group;
  }
}