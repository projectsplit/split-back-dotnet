using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> AddGuestToGroup(ObjectId groupId, string email, string nickname) {

    var newGuest = new Guest {
      UserId = ObjectId.GenerateNewId(),
      Email = email,
      Nickname = nickname,
      Roles = new List<ObjectId>()
    };

    var filter = Builders<Group>.Filter.Eq("_id", groupId);

    var groupUpdate = Builders<Group>.Update.Push("Guests", newGuest);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"group {groupId} not found");

    return Result.Success();
  }
}