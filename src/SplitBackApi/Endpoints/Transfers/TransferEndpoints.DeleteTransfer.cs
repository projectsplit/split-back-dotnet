using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using System.Security.Claims;

namespace SplitBackApi.Endpoints;

public static partial class TransferEndpoints {

  private static async Task<IResult> DeleteTransfer(
    ITransferRepository transferRepository,
    ClaimsPrincipal claimsPrincipal,
    DeleteTransferRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var currentTransferResult = await transferRepository.GetById(request.TransferId);
    if(currentTransferResult.IsFailure) Results.BadRequest(currentTransferResult.Error);
    var currentTransfer = currentTransferResult.Value;

    //TODO validate edited transfer

    var updateResult = await transferRepository.DeleteById(request.TransferId);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to delete transfer");

    return Results.Ok();
  }
}