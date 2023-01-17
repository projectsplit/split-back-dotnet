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
    await repo.RemoveTransfer(removeRestoreTransferDto.GroupId, removeRestoreTransferDto.TransferId);
    var result = await repo.GetGroupById(groupId);

    return result.Match(group => {
      return Results.Ok(group.PendingTransactions());
    }, e => {
      return Results.BadRequest(e.Message);
    });
  }
}
