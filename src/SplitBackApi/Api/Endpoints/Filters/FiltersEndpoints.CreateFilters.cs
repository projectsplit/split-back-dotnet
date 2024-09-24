using System.Security.Claims;
using MongoDB.Bson;
using SplitBackApi.Api.Endpoints.Filters.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupFiltersRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Filters;

public static partial class FiltersEndpoints
{
  private static async Task<IResult> CreateFilters(
      IGroupFiltersRepository filtersRepository,
      ClaimsPrincipal claimsPrincipal,
      CreateFiltersRequest request,
      CancellationToken ct
      )
  {
    var newFilter = new GroupFilter
    {
      GroupId = request.GroupId,
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      ParticipantsIds = request.ParticipantsIds,
      PayersIds = request.PayersIds,
      After = request.After,
      Before = request.Before,
      ReceiversIds = request.ReceiversIds,
      SendersIds = request.SendersIds
    };

    //Need to add a validator;
    var createResult = await filtersRepository.Create(newFilter, ct);

    return createResult.IsSuccess ? Results.Ok() : Results.BadRequest(createResult.Error);
  }
}