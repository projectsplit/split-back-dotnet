using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Endpoints.Transfers.Responses;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints
{

  private static async Task<IResult> GetTransfersByGroup(
    ITransferRepository transferRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    HttpRequest request
  )
  {
    var limitQuery = request.Query["limit"].ToString();
    if (string.IsNullOrEmpty(limitQuery)) return Results.BadRequest("limit query is missing");
    if (int.TryParse(limitQuery, out var limit) is false) return Results.BadRequest("invalid limit");

    var lastQuery = request.Query["last"].ToString();
    if (string.IsNullOrEmpty(lastQuery)) return Results.BadRequest("last query is missing");
    if (DateTime.TryParse(lastQuery, out var last) is false) return Results.BadRequest("invalid last");

    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("group query is missing");

    var transfersResult = await transferRepository.GetPaginatedTransfersByGroupId(groupId, limit, last);
    if (transfersResult.IsFailure) return Results.BadRequest(transfersResult.Error);

    var transfers = transfersResult.Value;

    var groupResult = await groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if (membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var response = transfers.Select(t => new TransferResponse
    {
      Amount = t.Amount,
      CreationTime = t.CreationTime,
      Currency = t.Currency,
      Description = t.Description,
      GroupId = t.GroupId,
      Id = t.Id,
      LastUpdateTime = t.LastUpdateTime,
      ReceiverId = t.ReceiverId,
      SenderId = t.SenderId,
      ReceiverName = membersWithNames.Single(m => m.Id == t.ReceiverId).Name,
      SenderName = membersWithNames.Single(m => m.Id == t.SenderId).Name
    });

    return Results.Ok(response);
  }
}