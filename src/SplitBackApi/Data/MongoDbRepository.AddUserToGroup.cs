using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task AddUserToGroup(IClientSessionHandle session, string groupId, string userId, ICollection<string> roleIds) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) & Builders<Group>.Filter.ElemMatch(x => x.Members, member => member.Id == userId);
    var userCount = await _groupCollection.CountDocumentsAsync(filter);

    var memberWithAcc = new MemberWithAccount {
      Id = ObjectId.GenerateNewId().ToString(),
      UserId = userId,
      Roles = roleIds
    };

    if(userCount == 0) {

      var updateGroup = Builders<Group>.Update.AddToSet("Members", memberWithAcc);
      await _groupCollection.FindOneAndUpdateAsync(session, group => group.Id == groupId, updateGroup);

      var updateUser = Builders<User>.Update.AddToSet("Groups", groupId);
      await _userCollection.FindOneAndUpdateAsync(session, user => user.Id == userId, updateUser);

    } else throw new Exception();
  }
}