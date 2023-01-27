namespace SplitBackApi.Extensions;

public static class MiddlewareExtensions {
  public static IApplicationBuilder UsePermissions(this IApplicationBuilder app) {
    return app.UseMiddleware<PermissionCheckMiddleware>();
  }
}
