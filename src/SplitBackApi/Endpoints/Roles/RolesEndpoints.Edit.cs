using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using MongoDB.Bson;
using SplitBackApi.Domain;

namespace SplitBackApi.Endpoints;
public static partial class RolesEndpoints {
  private static async Task<IResult> EditRole(IRepository repo, EditRoleRequest request) {

    var newRole = new Role {
      Title = request.Title,
      Permissions = request.Permissions.Cast<Permissions>().Aggregate((current, next) => current | next)
    };

    var editRoleResult = await repo.EditRole(ObjectId.Parse(request.RoleId), ObjectId.Parse(request.GroupId), request.Title, newRole);
    if(editRoleResult.IsFailure) return Results.BadRequest(editRoleResult.Error);

    return Results.Ok();
  }
}