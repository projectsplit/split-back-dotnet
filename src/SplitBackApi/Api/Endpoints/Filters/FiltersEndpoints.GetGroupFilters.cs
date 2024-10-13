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
      Participants = filters.ParticipantsIds.Select(participantId => new FilteredMember
      {
        MemberId = participantId,
        Value = membersWithNames.Single(mn => mn.Id == participantId).Name
      }).ToList(),
      Payers = filters.PayersIds.Select(payerId => new FilteredMember
      {
        MemberId = payerId,
        Value = membersWithNames.Single(mn => mn.Id == payerId).Name
      }).ToList(),

      Senders = filters.SendersIds.Select(senderId => new FilteredMember
      {
        MemberId = senderId,
        Value = membersWithNames.Single(mn => mn.Id == senderId).Name
      }).ToList(),
      Receivers = filters.ReceiversIds.Select(receiverId => new FilteredMember
      {
        MemberId = receiverId,
        Value = membersWithNames.Single(mn => mn.Id == receiverId).Name
      }).ToList()
    };
    return Results.Ok(response);

  }

}