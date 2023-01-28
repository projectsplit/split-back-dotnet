using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<User>> AddGroupInUserOrFail(ObjectId userId, ObjectId groupId) {
    
    var filter = 
      Builders<User>.Filter.Eq("_id", userId) &
      Builders<User>.Filter.Ne("Groups", groupId);
      
    var update = 
      Builders<User>.Update.AddToSet("Groups", groupId);
    
    var user = await _userCollection.FindOneAndUpdateAsync(filter, update);
    if(user is null) return Result.Failure<User>($"Group {groupId} already exists in user {userId}");
    
    return user;
  }
}