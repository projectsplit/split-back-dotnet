using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {

  private static async Task<IResult> DeleteTransfer(
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    DeleteTransferRequest request
  ) {

    var currentTransferResult = await transferRepository.GetById(request.TransferId);
    if(currentTransferResult.IsFailure) Results.BadRequest(currentTransferResult.Error);
    var currentTransfer = currentTransferResult.Value;

    var groupResult = await groupRepository.GetById(currentTransfer.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {currentTransfer.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    //TODO validate edited transfer

    var updateResult = await transferRepository.DeleteById(request.TransferId);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to delete transfer");

    return Results.Ok();
  }
}