using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using System.Security.Claims;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Verify(
    ClaimsPrincipal claimsPrincipal,
    IInvitationRepository invitationRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    VerifyInvitationRequest request
  ) {

    var invitationResult = await invitationRepository.GetByCode(request.Code);
    if(invitationResult.IsFailure) return Results.BadRequest(invitationResult.Error);
    var invitation = invitationResult.Value;

    if(invitation.Uses.Count >= invitation.NumberOfUses) {
      return Results.BadRequest("This invitation has been already used");
    }

    if(invitation.ExpirationTime < DateTime.UtcNow) {
      return Results.BadRequest("This invitation has been expired");
    }

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    if(invitation.InviterId == authenticatedUserId) {
      return Results.BadRequest("You can not invite yourself");
    }

    var inviterUserResult = await userRepository.GetById(invitation.InviterId);
    if(inviterUserResult.IsFailure) return Results.BadRequest(inviterUserResult.Error);
    var inviterUser = inviterUserResult.Value;

    var groupResult = await groupRepository.GetById(invitation.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is not null) return Results.BadRequest($"User {authenticatedUserId} is already a member of the group");

    if(invitation is ReplacementInvitation) {
      
      var replacementInvitation = (ReplacementInvitation)invitation;

      var memberToReplace = group.Members.Where(m => m.MemberId == replacementInvitation.MemberId).FirstOrDefault();
      if(memberToReplace is null) return Results.BadRequest($"Member {replacementInvitation.MemberId} has not been found");
    }

    return Results.Ok(new {
      InviterNickName = inviterUser.Nickname,
      GroupTitle = group.Title,
      GroupMembersCount = group.Members.Count
    });
  }
}