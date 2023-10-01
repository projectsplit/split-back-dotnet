using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Endpoints.Transfers.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> DeleteTransfer(
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    ClaimsPrincipal claimsPrincipal,
    DeleteTransferRequest request
  ) {

    var currentTransferResult = await transferRepository.GetById(request.TransferId);
    if(currentTransferResult.IsFailure) Results.BadRequest(currentTransferResult.Error);
    var currentTransfer = currentTransferResult.Value;

    var groupMaybe = await groupRepository.GetById(currentTransfer.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {currentTransfer.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    //TODO validate edited transfer

    var updateResult = await transferRepository.DeleteById(request.TransferId);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to delete transfer");

    var transfers = await transferRepository.GetByGroupIdPerPage(request.GroupId, 1, 20);

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