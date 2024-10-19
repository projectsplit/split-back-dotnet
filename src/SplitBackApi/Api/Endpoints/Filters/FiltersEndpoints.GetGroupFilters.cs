using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Filters.Responses;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupFiltersRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;

namespace SplitBackApi.Api.Endpoints.Filters;

public static partial class FiltersEndpoints
{
  private static async Task<IResult> GetGroupFilters(
        IGroupFiltersRepository filtersRepository,
        ClaimsPrincipal claimsPrincipal,
        HttpRequest request,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        CancellationToken ct
        )
  {
    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("Group id is missing");

    var groupResult = await groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var fitlerResult = await filtersRepository.GetByGroupId(groupId);
    if (fitlerResult.IsFailure) return Results.BadRequest(fitlerResult.Error);
    var filters = fitlerResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if (membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var response = new GetGroupFiltersResponse
    {
      Participants = filters?.ParticipantsIds.Select(participantId => new FilteredMember
      {
        MemberId = participantId,
        Value = membersWithNames.SingleOrDefault(mn => mn.Id == participantId)?.Name ?? string.Empty
      }).ToList() ?? new List<FilteredMember>(),

      Payers = filters?.PayersIds.Select(payerId => new FilteredMember
      {
        MemberId = payerId,
        Value = membersWithNames.SingleOrDefault(mn => mn.Id == payerId)?.Name ?? string.Empty
      }).ToList() ?? new List<FilteredMember>(),

      Senders = filters?.SendersIds.Select(senderId => new FilteredMember
      {
        MemberId = senderId,
        Value = membersWithNames.SingleOrDefault(mn => mn.Id == senderId)?.Name ?? string.Empty
      }).ToList() ?? new List<FilteredMember>(),

      Receivers = filters?.ReceiversIds.Select(receiverId => new FilteredMember
      {
        MemberId = receiverId,
        Value = membersWithNames.SingleOrDefault(mn => mn.Id == receiverId)?.Name ?? string.Empty
      }).ToList() ?? new List<FilteredMember>()
    };


    return Results.Ok(response);

  }

}