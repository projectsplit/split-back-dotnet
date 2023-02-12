using CSharpFunctionalExtensions;
using SplitBackApi.Data;
using SplitBackApi.Domain;

namespace SplitBackApi.Helper;



public static class InvitationHelper {

  public static async Task<Result> ProcessInvitation(string userId, Invitation invitation, IRepository repo) {

    await repo.CheckIfGuestInvitationAndAddUser(invitation, userId);
    await repo.CheckIfUserInvitationAndAddUser(invitation, userId);

    return Result.Success();
  }
}
