using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CheckIfUserInvitationAndAddUser(Invitation invitation, string userId) {

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    if(invitation is UserInvitation) {
      var userInvitation = (Invitation)invitation;
      try {

        await AddUserInGroupMembers(userId, userInvitation.GroupId);
        await AddGroupInUser(userId, userInvitation.GroupId);

      } catch(MongoException e) {

        await session.AbortTransactionAsync();
        return Result.Failure(e.ToString());

      }
    }

    return Result.Success();
  }
}
