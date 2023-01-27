using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> RestoreExpense(IRepository repo, RemoveRestoreExpenseDto removeRestoreExpenseDto) {

    var groupId = ObjectId.Parse(removeRestoreExpenseDto.GroupId);

    var restoreExpenseRes = await repo.RestoreExpense(removeRestoreExpenseDto.GroupId, removeRestoreExpenseDto.ExpenseId);
    if(restoreExpenseRes.IsFailure) return Results.BadRequest(restoreExpenseRes.Error);

    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());


  }
}