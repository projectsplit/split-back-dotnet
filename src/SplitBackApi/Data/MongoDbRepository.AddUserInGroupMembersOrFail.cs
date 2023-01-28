using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> AddUserInGroupMembersOrFail(ObjectId userId, ObjectId groupId) {

    var newMember = new Member {
      UserId = userId,
      Roles = new List<ObjectId>()
    };

    var filter =
      Builders<Group>.Filter.Eq("_id", groupId) &
      Builders<Group>.Filter.Ne("Members.$.UserId", userId);

    var updateGroup = Builders<Group>.Update.AddToSet("Members", newMember);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");
    
    return group;
  }
}