using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<DeleteResult> DeleteGuestInvitation(string inviterId, string groupId, string guestId) {

    var filter =
    Builders<Invitation>.Filter.Eq("Inviter", inviterId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GuestId", guestId.ToObjectId());

    return await _invitationCollection.DeleteManyAsync(filter);
  }
}