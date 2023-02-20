using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Invitation> GetGuestInvitationByInviterIdAndGuestId(string inviterId, string groupId, string guestId) {

    var filter =
      Builders<Invitation>.Filter.Eq("Inviter", inviterId) &
      Builders<Invitation>.Filter.Eq("Guest", guestId) &
      Builders<Invitation>.Filter.Eq("GroupId", groupId);

    return await _invitationCollection.Find(filter).FirstOrDefaultAsync();
  }
}