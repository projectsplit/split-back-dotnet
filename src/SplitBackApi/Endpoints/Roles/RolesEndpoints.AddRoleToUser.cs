using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;

public static partial class RolesEndpoints {

  private static async Task<IResult> AddRoleToUser(
    IRepository repo,
    AddRoleToUserRequest request,
    HttpContext httpContext,
    RoleService roleService
  ) {

    var addRoleToUserResult = await repo.AddRoleToUser(request.GroupId, request.UserId, request.RoleId);
    if(addRoleToUserResult.IsFailure) return Results.BadRequest(addRoleToUserResult.Error);

    return Results.Ok("Role added successfully");
  }
}