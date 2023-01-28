using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task CreateInvitation(ObjectId inviterId, ObjectId groupId) {
    
    var invitation = new Invitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = groupId,
      Inviter = inviterId,
      CreationTime = DateTime.UtcNow
    };
    
    await _invitationCollection.InsertOneAsync(invitation);
  }
}