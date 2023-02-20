using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> DeleteGuestInvitation(string inviterId, string groupId, string guestId) {

    var filter =
    Builders<Invitation>.Filter.Eq("Inviter", inviterId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GuestId", guestId.ToObjectId());

   var invitation = await _invitationCollection.DeleteManyAsync(filter);
   if(invitation is null) return Result.Failure($"Guest Invitation from inviter {inviterId}, for group {groupId} for guest {guestId} not found");

   return Result.Success();
  }
}