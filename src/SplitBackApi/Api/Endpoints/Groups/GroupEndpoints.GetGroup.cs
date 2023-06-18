using SplitBackApi.Api.Endpoints.Groups.Requests;
using SplitBackApi.Data.Repositories.GroupRepository;

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<IResult> GetGroup(
    // ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    GetGroupRequest request
  ) {
    var groupResult = await groupRepository.GetById(request.Id);
    
    if(groupResult.IsFailure) return Results.BadRequest("Group not found");
    
    return Results.Ok(groupResult.Value);
  }
}