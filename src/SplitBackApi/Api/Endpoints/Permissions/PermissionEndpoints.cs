namespace SplitBackApi.Api.Endpoints.Permissions;

public static partial class PermissionEndpoints {
  
  public static void MapPermissionEndpoints(this IEndpointRouteBuilder app) {
    
    var roleGroup = app.MapGroup("/permissions")
      .WithTags("Permissions");
      
    roleGroup.MapPost("/edit", Edit);
  }
}