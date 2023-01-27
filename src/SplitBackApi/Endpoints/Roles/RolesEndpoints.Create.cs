using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using MongoDB.Bson;
using SplitBackApi.Domain;

namespace SplitBackApi.Endpoints;
public static partial class RolesEndpoints {
  private static async Task<IResult> CreateRole(IRepository repo, CreateRoleRequest request) {

    //need to bring in list of integers as presented in permission class and then build a flags int here before creating the role.
    var newRole = new Role {
      Title = request.Title,
      Permissions = request.Permissions.Cast<Permissions>().Aggregate((current, next) => current | next) //request.Permissions.Select(p => (Permissions)p).ToList()
    };

    var createRoleResult = await repo.CreateRole(ObjectId.Parse(request.GroupId), request.Title, newRole);
    if(createRoleResult.IsFailure) return Results.BadRequest(createRoleResult.Error);

    return Results.Ok();
  }
}