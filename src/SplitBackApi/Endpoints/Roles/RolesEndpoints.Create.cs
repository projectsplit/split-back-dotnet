using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Domain;

namespace SplitBackApi.Endpoints;

public static partial class RolesEndpoints {
  
  private static async Task<IResult> CreateRole(
    IRepository repo,
    CreateRoleRequest request) {
    
    var newRoleTitle = request.Title;
    var newRolePermissions = 
      request.Permissions
      .Cast<Permissions>()
      .Aggregate((current, next) => current | next); //request.Permissions.Select(p => (Permissions)p).ToList()

    var createRoleResult = await repo.CreateRole(request.GroupId, request.Title, newRolePermissions);
    if(createRoleResult.IsFailure) return Results.BadRequest(createRoleResult.Error);

    return Results.Ok();
  }
}