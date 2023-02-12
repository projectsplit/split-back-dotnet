using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> AddUserInGroupMembers(string userId, string groupId) {

    var newMemberWithAcc = new MemberWithAccount {
      Id = ObjectId.GenerateNewId().ToString(),
      UserId = userId
    };

    var filter =
      Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) &
      Builders<Group>.Filter.Ne("Members.$.UserId", userId);

    var updateGroup = Builders<Group>.Update.AddToSet("Members", newMemberWithAcc);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");

    return group;
  }
}