using System.Net;
using MongoDB.Bson;
using SplitBackApi.Attributes;
using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Services;
using Newtonsoft.Json;
using NMoneys;

public class GroupPermissionsMiddleware : IMiddleware {

  private readonly RoleService _roleService;
  private readonly IRepository _repo;

  public GroupPermissionsMiddleware(RoleService roleService, IRepository repo) {

    _roleService = roleService;
    _repo = repo;

  }
  //How to get the country code when knowing longitude and latitude in Google Geocoding API
  public async Task InvokeAsync(HttpContext ctx, RequestDelegate next) {

    //var usd = Currency.Cad;

    var endpoint = ctx.GetEndpoint();
    var permissionsAttribute = endpoint?.Metadata.GetMetadata<PermissionAttribute>();

    if(permissionsAttribute is not null) {

      var requiredPermissions = permissionsAttribute.Permissions;

      var userId = ctx.GetAuthorizedUserId();

      var serializedRequest = ctx.SerializationAsync();

      var deserializedRequest = JsonConvert.DeserializeObject<GroupOperationRequestBase>(serializedRequest.Result);

      var groupId = ObjectId.Parse(deserializedRequest.GroupId);

      var groupResult = await _repo.GetGroupById(groupId);
      if(groupResult.IsFailure) { ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden; }

      var group = groupResult.Value;

      var permissionCheckResult = _roleService.MemberHasRequiredPermissions(userId, group, requiredPermissions);
      if(permissionCheckResult.IsFailure) { ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden; }

      if(permissionCheckResult.Value) {

        await next(ctx);

      } else {

        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      }

    } else {

      await next(ctx);
    }
  }
}