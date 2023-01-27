using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;


namespace SplitBackApi.Endpoints;
public static partial class TransferEndpoints {
  private static async Task<IResult> RestoreTransfer(IRepository repo, RemoveRestoreTransferDto removeRestoreTransferDto) {

    var groupId = ObjectId.Parse(removeRestoreTransferDto.GroupId);

    var restoreTransferRes = await repo.RestoreTransfer(removeRestoreTransferDto.GroupId, removeRestoreTransferDto.TransferId);
    if(restoreTransferRes.IsFailure) return Results.BadRequest(restoreTransferRes.Error);

    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());

  }
}
