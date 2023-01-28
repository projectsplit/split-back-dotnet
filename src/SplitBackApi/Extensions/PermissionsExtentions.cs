using SplitBackApi.Attributes;
using SplitBackApi.Domain;

namespace SplitBackApi.Extensions;

public static class RoutePermissionsExtensions {
  
  public static IEndpointConventionBuilder PermissionsRequired(this IEndpointConventionBuilder builder, Permissions permissions) {

    return builder.WithMetadata(new PermissionAttribute(permissions));
  }
}