using CSharpFunctionalExtensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> ProcessInvitation(string userId, Invitation invitation) {

    await CheckIfGuestInvitationAndAddUser(invitation, userId);
    await CheckIfUserInvitationAndAddUser(invitation, userId);

    return Result.Success();
  }
}
