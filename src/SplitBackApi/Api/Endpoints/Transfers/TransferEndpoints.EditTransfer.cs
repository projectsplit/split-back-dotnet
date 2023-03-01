using SplitBackApi.Api.Endpoints.Transfers.Requests;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Transfers;

public static partial class TransferEndpoints {
  
  private static async Task<IResult> EditTransfer(
    ITransferRepository transferRepository,
    EditTransferRequest request
  ) {

    var currentTransferResult = await transferRepository.GetById(request.TransferId);
    if(currentTransferResult.IsFailure) Results.BadRequest(currentTransferResult.Error);
    var currentTransfer = currentTransferResult.Value;

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

    //TODO validate edited transfer

    var updateResult = await transferRepository.Update(editedTransfer);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update transfer");

    return Results.Ok();
  }
}