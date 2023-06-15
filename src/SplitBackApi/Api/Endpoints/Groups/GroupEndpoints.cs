namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  public static void MapGroupEndpoints(this IEndpointRouteBuilder app) {

    var groupGroup = app.MapGroup("/group")
      .WithTags("Groups")
      .AllowAnonymous();

    groupGroup.MapPost("/creategroup", CreateGroup);
    groupGroup.MapGet("/getusergroups", GetUserGroups);
    groupGroup.MapPost("/getgroupbyid", GetGroupById);
  }
}