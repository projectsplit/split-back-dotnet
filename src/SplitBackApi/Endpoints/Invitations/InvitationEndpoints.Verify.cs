using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Verify(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings) {

    var authenticatedUserIdResult = httpContext.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var invitationResult = await repo.GetInvitationByCode(request.Code);
    if(invitationResult.IsFailure) return Results.BadRequest(invitationResult.Error);
    var invitation = invitationResult.Value;
    
    var groupResult = await repo.GetGroupIfUserIsNotMember(authenticatedUserId, invitation.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var inviterResult = await repo.GetUserById(invitation.Inviter);
    if(inviterResult.IsFailure) return Results.BadRequest(inviterResult.Error);
    var inviter = inviterResult.Value;

    var userResult = await repo.GetUserIfGroupNotExistsInUserGroups(authenticatedUserId, invitation.GroupId);
    if(userResult.IsFailure) return Results.BadRequest(userResult.Error);

    return Results.Ok(new {
      Message = "Invitation is valid",
      InviterNickName = inviter.Nickname,
      group = group.Title
    });
  }
}