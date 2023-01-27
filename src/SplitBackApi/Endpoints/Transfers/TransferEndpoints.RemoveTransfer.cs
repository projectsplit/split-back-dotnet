using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;
public static partial class TransferEndpoints {
  private static async Task<IResult> RemoveTransfer(IRepository repo, RemoveRestoreTransferDto removeRestoreTransferDto) {

    var groupId = ObjectId.Parse(removeRestoreTransferDto.GroupId);

    var removeTransferRes = await repo.RemoveTransfer(removeRestoreTransferDto.GroupId, removeRestoreTransferDto.TransferId);
    if(removeTransferRes.IsFailure) return Results.BadRequest(removeTransferRes.Error);

    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());

  }
}
