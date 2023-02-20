using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task CreateUserInvitation(string inviterId, string groupId) {

    var invitation = new UserInvitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = groupId,
      Inviter = inviterId,
      CreationTime = DateTime.UtcNow
    };

    await _invitationCollection.InsertOneAsync(invitation);
  }
}