using System.Security.Claims;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;


// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetUserGroups(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groupsResult = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if(groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;

    return Results.Ok(groups);
  }
}