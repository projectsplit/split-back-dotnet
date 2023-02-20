using SplitBackApi.Data;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;


namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Accept(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings
  ) {

    var authenticatedUserIdResult = httpContext.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var getInvitationResult = await repo.GetInvitationByCode(request.Code);
    if(getInvitationResult.IsFailure) return Results.BadRequest(getInvitationResult.Error);
    var invitation = getInvitationResult.Value;

    var groupResult = await repo.GetGroupById(invitation.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var memberFound = group.GetUserMemberByUserId(authenticatedUserId);

    if(memberFound is not null) return Results.BadRequest($"User {authenticatedUserId} is already a member of the group");


    switch(invitation) {

      case GuestInvitation: {

          var guestInvitation = (GuestInvitation)invitation;
          var guestFound = group.GetGuestMemberByGuestId(guestInvitation.GuestId);

          if(guestFound is null) return Results.BadRequest($"Guest {guestInvitation.GuestId} is not a member of the group");

          var userMember = new UserMember {
            Id = guestFound.Id,
            UserId = authenticatedUserId,
            Roles = guestFound.Roles
          };

          await repo.ReplaceGuestMemberWithUserMember(group.Id, userMember, guestInvitation.GuestId);
          break;
        }

      case UserInvitation: {

          await repo.AddUserMemberToGroup(group.Id, authenticatedUserId, new List<string>());
          break;
        }
        
      default:
        return Results.BadRequest("Not a valid invitation");
    }

    return Results.Ok(new {
      Message = $"User {authenticatedUserId} joined group",

    });
  }
}