using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;

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

    var inviterResult = await repo.GetUserById(invitation.Inviter);
    if(inviterResult.IsFailure) return Results.BadRequest(inviterResult.Error);
    var inviter = inviterResult.Value;

    var groupResult = await repo.GetGroupById(invitation.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    switch(invitation) {

      case GuestInvitation: {

          var guestInvitation = (GuestInvitation)invitation;
          var userMember = group.GetUserMemberByUserId(authenticatedUserId);
          if(userMember is not null) return Results.BadRequest($"User {authenticatedUserId} is already a member of the group");

          var guestMember = group.GetUserMemberByUserId(guestInvitation.GuestId);
          if(guestMember is null) return Results.BadRequest($"Guest {guestInvitation.GuestId} was not found");

          break;
        }

      case UserInvitation: {
        
          var userMember = group.GetUserMemberByUserId(authenticatedUserId);
          if(userMember is not null) return Results.BadRequest($"User {authenticatedUserId} is already a member of the group");

          break;
        }
      default:
        return Results.BadRequest("Not a valid invitation");
    }

    var userResult = await repo.GetUserById(authenticatedUserId);
    var user = userResult.Value;
    var groupFound = user.Groups.Contains(invitation.GroupId);
    if(groupFound) return Results.BadRequest($"Group{invitation.GroupId} already exists in user {authenticatedUserId}");

    return Results.Ok(new {
      Message = "Invitation is valid",
      InviterNickName = inviter.Nickname,
      group = group.Title
    });
  }
}