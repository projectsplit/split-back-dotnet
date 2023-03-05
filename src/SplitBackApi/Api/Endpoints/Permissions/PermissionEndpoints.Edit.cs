using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Permissions.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Permissions;

public static partial class PermissionEndpoints {

  private static async Task<IResult> Edit(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    GroupValidator groupValidator,
    UserMemberValidator userMemberValidator,
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

    var memberToEdit = group.Members.Where(m => m.MemberId == request.MemberId).First();
    memberToEdit.Permissions = request.Permissions;

    if(memberToEdit is not UserMember) return Results.BadRequest();

    var userMemberValidationResult = userMemberValidator.Validate(memberToEdit as UserMember);
    if(userMemberValidationResult.IsValid is false) return Results.BadRequest(userMemberValidationResult.ToErrorResponse());

    group.LastUpdateTime = DateTime.UtcNow;

    var groupValidationResult = groupValidator.Validate(group);
    if(groupValidationResult.IsValid is false) return Results.BadRequest(groupValidationResult.ToErrorResponse());

    var updateResult = await groupRepository.Update(group);
    if(updateResult.IsFailure) return Results.BadRequest(updateResult.Error);

    return Results.Ok();
  }
}