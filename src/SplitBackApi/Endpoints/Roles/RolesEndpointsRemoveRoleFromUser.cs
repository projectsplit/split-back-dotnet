using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Extensions;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;
public static partial class RolesEndpoints {
  private static async Task<IResult> RemoveRoleFromUser(
  IRepository repo,
  RemoveRoleFromUserRequest request,
  HttpContext httpContext,
  RoleService roleService) {
    
    //var authedUserId = httpContext.GetAuthorizedUserId();
    try {

      var userId = ObjectId.Parse(request.UserId);
      var groupId = ObjectId.Parse(request.GroupId);
      var roleId = ObjectId.Parse(request.RoleId);

      var removeRoleFromuserResult = await repo.RemoveRoleFromUser(groupId, userId, roleId);
      if(removeRoleFromuserResult.IsFailure) return Results.BadRequest(removeRoleFromuserResult.Error);

    } catch(ObjectIdParceException ex) {

      return Results.BadRequest(ex.Message);
    }

    return Results.Ok("Role removed successfully");
  }
}