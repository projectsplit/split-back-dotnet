using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CreateRole(string groupId, string roleTitle, Permissions rolePermissions) {
    
    var newRole = new Role {
      Id = ObjectId.GenerateNewId().ToString(),
      Title = roleTitle,
      Permissions = rolePermissions
    };
    
    var filter = Builders<Group>.Filter.Eq("_id", groupId.ToObjectId());
    var groupUpdate = Builders<Group>.Update.Push("Roles", newRole);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}