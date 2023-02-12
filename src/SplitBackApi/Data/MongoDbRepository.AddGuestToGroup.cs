using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> AddGuestToGroup(string groupId, string email, string nickname) {

    var newGuest = new Guest {
      Id = ObjectId.GenerateNewId().ToString(),
      Name = nickname
    };

    var filter = Builders<Group>.Filter.Eq("_id", groupId.ToObjectId());

    var groupUpdate = Builders<Group>.Update.Push("Members", newGuest);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"group {groupId} not found");

    return Result.Success();
  }
}