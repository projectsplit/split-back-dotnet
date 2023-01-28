using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransferEndpoints {
  
  private static async Task<IResult> RestoreTransfer(IRepository repo, RemoveRestoreTransferDto removeRestoreTransferDto) {

    var groupId = ObjectId.Parse(removeRestoreTransferDto.GroupId);

    var restoreTransferResult = await repo.RestoreTransfer(removeRestoreTransferDto.GroupId, removeRestoreTransferDto.TransferId);
    if(restoreTransferResult.IsFailure) return Results.BadRequest(restoreTransferResult.Error);

    var getGroupResult = await repo.GetGroupById(groupId);
    if(getGroupResult.IsFailure) return Results.BadRequest(getGroupResult.Error);
    var group = getGroupResult.Value;
    
    return Results.Ok(group.PendingTransactions());
  }
}
