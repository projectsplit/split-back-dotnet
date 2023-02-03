using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task AddUserToGroup(IClientSessionHandle session, ObjectId groupID, ObjectId userID, ICollection<ObjectId> roleIDs) {

    var filter = Builders<Group>.Filter.Eq("_id", groupID) & Builders<Group>.Filter.ElemMatch(x => x.Members, member => member.UserId == userID);
    var userCount = await _groupCollection.CountDocumentsAsync(filter);

    var member = new Member {
      UserId = userID,
      Roles = roleIDs
    };

    if(userCount == 0) {

      var updateGroup = Builders<Group>.Update.AddToSet("Members", member);
      await _groupCollection.FindOneAndUpdateAsync(session, group => group.Id == groupID, updateGroup);

      var updateUser = Builders<User>.Update.AddToSet("Groups", groupID);
      await _userCollection.FindOneAndUpdateAsync(session, user => user.Id == userID, updateUser);

    } else throw new Exception();
  }
}