using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CheckIfGuestInvitationAndAddUser(Invitation invitation, string userId) {

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    if(invitation is GuestInvitation) {

      var guestInvitation = (GuestInvitation)invitation;
      try {

        await RemoveGuestFromGroup(guestInvitation.GroupId, guestInvitation.GuestId);
        await AddUserInGroupMembersByRetainingGuestId(userId, guestInvitation.GroupId, guestInvitation.GuestId);
        await AddGroupInUser(userId, guestInvitation.GroupId);

      } catch(MongoException e) {

        await session.AbortTransactionAsync();
        return Result.Failure(e.ToString());

      }
    }
    return Result.Success();
  }
}
