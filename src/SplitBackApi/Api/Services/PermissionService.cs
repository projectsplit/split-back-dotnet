using System.Security.Claims;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Services;

public class PermissionService {

  public async Task<IResult> CheckPermissions(string groupId, ClaimsPrincipal claimsPrincipal, IGroupRepository groupRepository, Permissions permission) {

    var groupResult = await groupRepository.GetById(groupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {groupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.Comment) is false) return Results.Forbid();

    return Results.Ok();
  }
}
