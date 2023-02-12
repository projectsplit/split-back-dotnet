using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;

namespace SplitBackApi.Endpoints;

public static partial class GuestInvitationEndpoints {

  private static async Task<IResult> Verify(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings) {

    var authenticatedUserIdResult = httpContext.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var getGuestInvitationResult = await repo.GetGuestInvitationByCode(request.Code);
    if(getGuestInvitationResult.IsFailure) return Results.BadRequest(getGuestInvitationResult.Error);
    var guestInvitation = getGuestInvitationResult.Value;

    var inviterResult = await repo.GetUserById(guestInvitation.Inviter);
    if(inviterResult.IsFailure) return Results.BadRequest(inviterResult.Error);
    var inviter = inviterResult.Value;

    var groupResult = await repo.GetGroupIfUserAndGuestCriteriaAreSatisified(authenticatedUserId, guestInvitation);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var userResult = await repo.GetUserIfGroupNotExistsInUserGroups(authenticatedUserId, guestInvitation.GroupId);
    if(userResult.IsFailure) return Results.BadRequest(userResult.Error);

    return Results.Ok(new {
      Message = "Guest Invitation is valid",
      InviterNickName = inviter.Nickname,
      group = group.Title
    });
  }
}