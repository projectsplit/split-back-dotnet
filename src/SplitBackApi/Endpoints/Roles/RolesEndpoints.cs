using SplitBackApi.Domain;
using SplitBackApi.Extensions;
namespace SplitBackApi.Endpoints;
public static partial class RolesEndpoints {
  public static void MapRolesEndpoints(this IEndpointRouteBuilder app) {
    var roleGroup = app.MapGroup("/role")
      .WithTags("Roles")
      .AllowAnonymous();
    roleGroup.MapPost("/create", CreateRole);
    roleGroup.MapPost("/edit", EditRole);
    roleGroup.MapPost("/addRoleToUser", AddRoleToUser);
    roleGroup.MapPost("/removeRoleFromUser", RemoveRoleFromUser);
  }
}