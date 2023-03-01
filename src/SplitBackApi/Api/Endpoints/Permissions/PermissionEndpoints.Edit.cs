using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Permissions.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Api.Endpoints.Permissions;

public static partial class PermissionEndpoints {

  private static async Task<IResult> Edit(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    EditPermissionsRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var member = group.Members.FirstOrDefault(m => m.MemberId == request.MemberId);
    if(member is null) {
      return Results.BadRequest($"Group with id {request.GroupId} does not include a member with id {request.MemberId}");
    }

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var authenticatedUserMember = group.GetMemberByUserId(authenticatedUserId);
    if(authenticatedUserMember is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    // you try to change your own permissions
    if(authenticatedUserMember.MemberId == request.MemberId) return Results.Forbid();

    // you dont have "ManageGroup" permission
    if(authenticatedUserMember.Permissions.HasFlag(Domain.Models.Permissions.ManageGroup) is false) {
      return Results.Forbid();
    }

    // try to change an admin's permissions when you are not a group owner
    if(member.Permissions.HasFlag(Domain.Models.Permissions.ManageGroup) && group.OwnerId != authenticatedUserId) {
      return Results.Forbid();
    }
    
    // try to create an admin when you are not a group owner
    if(member.Permissions.HasFlag(Domain.Models.Permissions.ManageGroup) is false &&
      request.Permissions.HasFlag(Domain.Models.Permissions.ManageGroup) &&
      group.OwnerId != authenticatedUserId) return Results.Forbid();

    group.Members.Where(m => m.MemberId == request.MemberId).First().Permissions = request.Permissions;
    group.LastUpdateTime = DateTime.UtcNow;

    var updateResult = await groupRepository.Update(group);
    if(updateResult.IsFailure) return Results.BadRequest(updateResult.Error);

    return Results.Ok();
  }
}