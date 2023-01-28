using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CreateRole(ObjectId groupId, string roleName, Role newRole) {
    
    var filter = Builders<Group>.Filter.Eq("_id", groupId);
    var groupUpdate = Builders<Group>.Update.Push("Roles", newRole);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}