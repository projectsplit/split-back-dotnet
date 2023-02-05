using System.Net;
using SplitBackApi.Attributes;
using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Services;
using Newtonsoft.Json;

public class GroupPermissionsMiddleware : IMiddleware {

  private readonly RoleService _roleService;
  private readonly IRepository _repo;

  public GroupPermissionsMiddleware(RoleService roleService, IRepository repo) {

    _roleService = roleService;
    _repo = repo;
  }

  public async Task InvokeAsync(HttpContext context, RequestDelegate next) {

    var endpoint = context.GetEndpoint();
    var permissionsAttribute = endpoint?.Metadata.GetMetadata<PermissionAttribute>();

    if(permissionsAttribute is null) {
      await next(context);
      return;
    }

    var serializedRequest = await context.Request.Body.ToSerializedString();
    var deserializedRequest = JsonConvert.DeserializeObject<GroupOperationRequestBase>(serializedRequest);

    if(deserializedRequest is null) {
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return;
    }

    var groupResult = await _repo.GetGroupById(deserializedRequest.GroupId);

    if(groupResult.IsFailure) {
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return;
    }

    var userId = context.GetAuthorizedUserId().Value;
    var group = groupResult.Value;
    var requiredPermissions = permissionsAttribute.Permissions;
    
    var permissionCheckResult = _roleService.MemberHasRequiredPermissions(userId, group, requiredPermissions);

    if(permissionCheckResult.IsFailure) {
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return;
    }

    if(!permissionCheckResult.Value) {
      context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      return;
    }

    await next(context);
  }
}