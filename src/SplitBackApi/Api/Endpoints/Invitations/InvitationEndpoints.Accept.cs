using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Invitations.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.InvitationRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Invitations;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Accept(
    ClaimsPrincipal claimsPrincipal,
    IUserRepository userRepository,
    IInvitationRepository invitationRepository,
    IGroupRepository groupRepository,
    UserMemberValidator userMemberValidator,
    GroupValidator groupValidator,
    AcceptInvitationRequest request
  ) {

    var invitationResult = await invitationRepository.GetByCode(request.Code);
    if(invitationResult.IsFailure) return Results.BadRequest(invitationResult.Error);
    var invitation = invitationResult.Value;

    if(invitation.Uses.Count >= invitation.NumberOfUses) {
      return Results.BadRequest("This invitation has been already used");
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

      group.Members.Remove(memberToReplace);
      group.Members.Add(new UserMember {
        MemberId = Guid.NewGuid().ToString(),
        UserId = authenticatedUserId,
        Permissions = Domain.Models.Permissions.Comment | Domain.Models.Permissions.CreateInvitation | Domain.Models.Permissions.WriteAccess//memberToReplace.Permissions
      });

    } else {
      
      var newUserMember = new UserMember {
        MemberId = Guid.NewGuid().ToString(),
        UserId = authenticatedUserId,
        Permissions = Domain.Models.Permissions.Comment | Domain.Models.Permissions.CreateInvitation | Domain.Models.Permissions.WriteAccess
      };
      
      var validationResult = userMemberValidator.Validate(newUserMember);
      if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

      group.Members.Add(newUserMember);
    }

    invitation.Uses.Add(new InvitationUse {
      UserId = authenticatedUserId,
      UseTime = DateTime.UtcNow
    });
    
    var groupValidationResult = groupValidator.Validate(group);
    if(groupValidationResult.IsValid is false ) return Results.BadRequest(groupValidationResult.ToErrorResponse());

    // TODO use transaction somehow
    var invitationUpdateResult = await invitationRepository.Update(invitation);
    var groupUpdateResult = await groupRepository.Update(group);

    if(CSharpFunctionalExtensions.Result.Combine(invitationUpdateResult, groupUpdateResult).IsFailure) {
      return Results.BadRequest(groupUpdateResult.Error);
    }

    return Results.Ok();
  }
}