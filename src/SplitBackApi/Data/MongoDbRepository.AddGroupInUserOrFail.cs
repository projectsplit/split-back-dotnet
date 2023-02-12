using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<User>> AddGroupInUser(string userId, string groupId) {

    
    var filter =
      Builders<User>.Filter.Eq("_id", userId.ToObjectId()) &
      Builders<User>.Filter.Ne("Groups", groupId.ToObjectId());

    var update = Builders<User>.Update.AddToSet("Groups", groupId);

    var user = await _userCollection.FindOneAndUpdateAsync(filter, update);
    if(user is null) return Result.Failure<User>($"Group {groupId} already exists in user {userId}");

    return user;
  }
}