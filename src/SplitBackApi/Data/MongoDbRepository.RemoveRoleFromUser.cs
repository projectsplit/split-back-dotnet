using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
    public async Task<Result> RemoveRoleFromUser(string groupId, string userId, string roleId) {

    var filter = 
      Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) & 
      Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.Id == userId);
      
    var groupUpdate = Builders<Group>.Update.Pull("Members.$.Roles", roleId);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}