using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task CreateGuestInvitation(string inviterId, string groupId, string guestId) {

    var invitation = new GuestInvitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = groupId,
      Inviter = inviterId,
      GuestId = guestId,
      CreationTime = DateTime.UtcNow
    };

    await _invitationCollection.InsertOneAsync(invitation);
  }
}