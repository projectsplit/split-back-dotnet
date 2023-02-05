using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<DeleteResult> DeleteInvitation(string userId, string groupId) {
    
    var filter = Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId);
    
    return await _invitationCollection.DeleteManyAsync(filter);
  }
}