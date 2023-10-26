using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Endpoints.Transfers.Responses;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> GetTransfersByGroup(
    ITransferRepository transferRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    GetTransfersByGroupRequest request
  ) {

    var transfers = await transferRepository.GetByGroupIdPerPage(request.GroupId, request.PageNumber, request.PageSize);

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if(membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var response = transfers.Select(t => new TransferResponse {
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