using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result> AddRoleToUser(string groupId, string userId, string roleId) {

    var filter = 
      Builders<Group>.Filter.Eq("_id", groupId) & 
      Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.Id == userId);
      
    var groupUpdate = Builders<Group>.Update.AddToSet("Members.$.Roles", roleId);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}