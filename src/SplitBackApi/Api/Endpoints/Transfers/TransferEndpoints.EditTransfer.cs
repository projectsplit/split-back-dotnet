using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> EditTransfer(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    ITransferRepository transferRepository,
    TransferValidator transferValidator,
    EditTransferRequest request
  ) {

    var currentTransferResult = await transferRepository.GetById(request.TransferId);
    if(currentTransferResult.IsFailure) Results.BadRequest(currentTransferResult.Error);
    var currentTransfer = currentTransferResult.Value;

    var groupMaybe = await groupRepository.GetById(currentTransfer.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var groupMemberIds = group.Members.Select(m => m.MemberId).ToList();

    var requestMemberIds = new List<string> { request.ReceiverId, request.SenderId };
    var memberIdsAreValid = requestMemberIds.Intersect(groupMemberIds).Count() == requestMemberIds.Count();
    if(memberIdsAreValid is not true) return Results.BadRequest("Sender and/ or Receiver Id(s) are not valid");

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {currentTransfer.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    var editedTransfer = new Transfer {
      Id = currentTransfer.Id,
      CreationTime = currentTransfer.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      Amount = request.Amount,
      Currency = request.Currency,
      Description = request.Description,
      GroupId = currentTransfer.GroupId,
      ReceiverId = request.ReceiverId,
      SenderId = request.SenderId,
      TransferTime = request.TransferTime
    };

    var validationResult = transferValidator.Validate(editedTransfer);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var updateResult = await transferRepository.Update(editedTransfer);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update transfer");

    return Results.Ok();
  }
}