using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
    public async Task<Result> RemoveRoleFromUser(ObjectId groupId, ObjectId userId, ObjectId roleId) {

    var filter = 
      Builders<Group>.Filter.Eq("_id", groupId) & 
      Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.UserId == userId);
      
    var groupUpdate = Builders<Group>.Update.Pull("Members.$.Roles", roleId);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, groupUpdate);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}