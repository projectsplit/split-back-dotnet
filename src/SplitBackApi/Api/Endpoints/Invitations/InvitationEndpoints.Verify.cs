using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Invitations.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.InvitationRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Invitations;

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
      return Results.BadRequest("This invitation has already been used");
    }

    if(invitation.ExpirationTime < DateTime.UtcNow) {
      return Results.BadRequest("This invitation has expired");
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