using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;

using AutoMapper;

namespace SplitBackApi.Endpoints;
public static partial class GroupEndpoints {
  private static async Task<IResult> CreateGroup(HttpContext httpContext, IRepository repo, IMapper mapper, CreateGroupDto createGroupDto) {
    
    try {
      var authedUserId = httpContext.GetAuthorizedUserId();
      var group = mapper.Map<Group>(createGroupDto);
      group.CreatorId = authedUserId;
      var createGroupResult = await repo.CreateGroup(group);
      if(createGroupResult.IsFailure) return Results.BadRequest(createGroupResult.Error);

      return Results.Ok();
    } catch(Exception ex) {
      return Results.BadRequest(ex.Message);
    }
  }
}