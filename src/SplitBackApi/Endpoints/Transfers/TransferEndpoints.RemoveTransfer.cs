using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransferEndpoints {
  
  private static async Task<IResult> RemoveTransfer(IRepository repo, RemoveRestoreTransferDto removeRestoreTransferDto) {

    var removeTransferResult = await repo.RemoveTransfer(removeRestoreTransferDto.GroupId, removeRestoreTransferDto.TransferId);
    if(removeTransferResult.IsFailure) return Results.BadRequest(removeTransferResult.Error);

    var getGroupResult = await repo.GetGroupById(removeRestoreTransferDto.GroupId);
    if(getGroupResult.IsFailure) return Results.BadRequest(getGroupResult.Error);
    var group = getGroupResult.Value;
    
    return Results.Ok(group.PendingTransactions());
  }
}
