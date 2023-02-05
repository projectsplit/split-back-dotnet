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
  public async Task InvokeAsync(HttpContext context, RequestDelegate next) {

    //var usd = Currency.Cad;

    var endpoint = context.GetEndpoint();
    var permissionsAttribute = endpoint?.Metadata.GetMetadata<PermissionAttribute>();

    if(permissionsAttribute is not null) {

      var requiredPermissions = permissionsAttribute.Permissions;

      var userId = context.GetAuthorizedUserId().Value;

      var serializedRequest = context.Request.Body.ToSerializedString();

      var deserializedRequest = JsonConvert.DeserializeObject<GroupOperationRequestBase>(serializedRequest.Result);

      var groupResult = await _repo.GetGroupById(deserializedRequest.GroupId);
      if(groupResult.IsFailure) { context.Response.StatusCode = (int)HttpStatusCode.Forbidden; }

      var group = groupResult.Value;

      var permissionCheckResult = _roleService.MemberHasRequiredPermissions(userId, group, requiredPermissions);
      if(permissionCheckResult.IsFailure) { context.Response.StatusCode = (int)HttpStatusCode.Forbidden; }

      if(permissionCheckResult.Value) {

        await next(context);

      } else {

        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      }

    } else {

      await next(context);
    }
  }
}