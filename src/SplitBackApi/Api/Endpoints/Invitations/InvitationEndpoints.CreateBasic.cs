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

  private static async Task<IResult> CreateBasic(
    IGroupRepository groupRepository,
    IInvitationRepository invitationRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateBasicInvitationRequest request
  ) {

    var groupMaybe = await groupRepository.GetById(request.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.CreateInvitation) is false) return Results.Forbid();

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

    return Results.Ok(newBasicInvitation.Code);
  }
}