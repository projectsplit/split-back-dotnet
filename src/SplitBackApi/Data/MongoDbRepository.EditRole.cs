using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result> EditRole(string roleId, string groupId, string roleName, Role newRole) {

    var filter = 
      Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) & 
      Builders<Group>.Filter.ElemMatch(g => g.Roles, r => r.Id == roleId);
    
    var groupUpdate = Builders<Group>.Update
      .Set("Roles.$.Title", newRole.Title)
      .Set("Roles.$.Permissions", newRole.Permissions);

    //var group = _groupCollection.Find<Group>(g => g.Id == groupId).ToList();
    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}