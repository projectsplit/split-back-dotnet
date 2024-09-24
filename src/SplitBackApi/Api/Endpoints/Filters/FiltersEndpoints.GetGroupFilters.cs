using System.Security.Claims;
using SplitBackApi.Data.Repositories.GroupFiltersRepository;

namespace SplitBackApi.Api.Endpoints.Filters;

public static partial class FiltersEndpoints
{
  private static async Task<IResult> GetGroupFilters(
        IGroupFiltersRepository filtersRepository,
        ClaimsPrincipal claimsPrincipal,
        HttpRequest request,
        CancellationToken ct
        )
  {
    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("Group id is missing");

    var fitlerResult = await filtersRepository.GetByGroupId(groupId);
    if (fitlerResult.IsFailure) return Results.BadRequest(fitlerResult.Error);
    var fitlers = fitlerResult.Value;

    return Results.Ok(fitlers);

  }

}