using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Invitations.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.InvitationRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Helper;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Invitations;

public static partial class InvitationEndpoints {

  private static async Task<IResult> CreateReplacement(
    IGroupRepository groupRepository,
    IInvitationRepository invitationRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateReplacementInvitationRequest request
  ) {
      
    var groupMaybe = await groupRepository.GetById(request.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    var memberToReplace = group.Members.Where(m => m.MemberId == request.MemberId).FirstOrDefault();
    if(memberToReplace is null) return Results.BadRequest($"Member with id {request.MemberId} does not belong to group {request.GroupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.CreateInvitation) is false) return Results.Forbid();

    var newReplacementInvitation = new ReplacementInvitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = request.GroupId,
      InviterId = authenticatedUserId,
      ExpirationTime = DateTime.UtcNow.AddDays(1),
      NumberOfUses = 1,
      MemberId = request.MemberId,
      Uses = new List<InvitationUse>(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow
    };

    await invitationRepository.Create(newReplacementInvitation);

    return Results.Ok();
  }
}