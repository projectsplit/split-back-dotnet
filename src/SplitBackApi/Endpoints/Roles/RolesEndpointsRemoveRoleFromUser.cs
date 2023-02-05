using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;

public static partial class RolesEndpoints {

  private static async Task<IResult> RemoveRoleFromUser(
    IRepository repo,
    RemoveRoleFromUserRequest request,
    HttpContext httpContext,
    RoleService roleService
  ) {

    var removeRoleFromuserResult = await repo.RemoveRoleFromUser(request.GroupId, request.UserId, request.RoleId);
    if(removeRoleFromuserResult.IsFailure) return Results.BadRequest(removeRoleFromuserResult.Error);

    return Results.Ok("Role removed successfully");
  }
}