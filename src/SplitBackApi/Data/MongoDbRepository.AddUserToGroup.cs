using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task AddUserToGroup(ObjectId groupID, ObjectId userID) {
    
    var filter = Builders<Group>.Filter.Eq("_id", groupID) & Builders<Group>.Filter.AnyEq("Members.$.UserId", userID);
    var userCount = await _groupCollection.CountDocumentsAsync(filter);

    if(userCount == 0) {
      
      var updateGroup = Builders<Group>.Update.AddToSet("Members", userID);
      await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupID, updateGroup);
      
      var updateUser = Builders<User>.Update.AddToSet("Groups", groupID);
      await _userCollection.FindOneAndUpdateAsync(user => user.Id == userID, updateUser);
      
    } else throw new Exception();
  }
}