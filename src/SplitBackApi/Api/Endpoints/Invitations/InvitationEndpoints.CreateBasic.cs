using SplitBackApi.Data;
using SplitBackApi.Requests;
using System.Security.Claims;
using SplitBackApi.Extensions;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain;
using SplitBackApi.Helper;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> CreateBasic(
    IGroupRepository groupRepository,
    IInvitationRepository invitationRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateBasicInvitationRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(member.Permissions.HasFlag(Permissions.CreateInvitation) is false) return Results.Forbid();

    var newBasicInvitation = new Invitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = request.GroupId,
      InviterId = authenticatedUserId,
      ExpirationTime = DateTime.UtcNow.AddDays(1),
      NumberOfUses = 5,
      Uses = new List<InvitationUse>(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow
    };

    await invitationRepository.Create(newBasicInvitation);

    return Results.Ok();
  }
}