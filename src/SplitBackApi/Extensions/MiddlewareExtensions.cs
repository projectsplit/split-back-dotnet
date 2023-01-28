namespace SplitBackApi.Extensions;

public static class MiddlewareExtensions {
  
  public static IApplicationBuilder UseGroupPermissionMiddleware(this IApplicationBuilder app) {
    
    return app.UseMiddleware<GroupPermissionsMiddleware>();
  }
}