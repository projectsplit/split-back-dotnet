using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RegenerateGuestInvitation(string inviterId, string groupId, string guestId) {

    var filter =
      Builders<Invitation>.Filter.Eq("Inviter", inviterId.ToObjectId()) &
      Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId()) &
      Builders<Invitation>.Filter.Eq("GuestId", guestId.ToObjectId());

    var invitationUpdate =
      Builders<Invitation>.Update.Set("Code", InvitationCodeGenerator.GenerateInvitationCode());

    var invitation = await _invitationCollection.UpdateOneAsync(filter, invitationUpdate);

    return Result.Success();
  }
}