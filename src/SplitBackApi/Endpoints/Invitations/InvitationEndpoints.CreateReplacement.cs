using SplitBackApi.Data;
using SplitBackApi.Requests;
using System.Security.Claims;
using SplitBackApi.Extensions;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> CreateReplacement(
    IGroupRepository groupRepository,
    IInvitationRepository invitationRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateReplacementInvitationRequest request
  ) {
      
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    var memberToReplace = group.Members.Where(m => m.MemberId == request.MemberId).FirstOrDefault();
    if(memberToReplace is null) return Results.BadRequest($"Member with id {request.MemberId} does not belong to group {request.GroupId}");

    if(member.Permissions.HasFlag(Permissions.CreateInvitation) is false) return Results.Forbid();

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