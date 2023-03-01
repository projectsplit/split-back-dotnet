using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.TransferRepository;

namespace SplitBackApi.Api.Endpoints.Transfers;

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