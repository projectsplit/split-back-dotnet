using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<DeleteResult> DeleteUserInvitation(string userId, string groupId) {

    var filter =
    Builders<Invitation>.Filter.Eq("Inviter", userId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId());

    return await _invitationCollection.DeleteManyAsync(filter);
  }
}