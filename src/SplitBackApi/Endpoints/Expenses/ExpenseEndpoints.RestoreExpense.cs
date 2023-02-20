using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> RestoreExpense(IRepository repo, RemoveRestoreExpenseDto removeRestoreExpenseDto) {

    var restoreExpenseRes = await repo.RestoreExpense(removeRestoreExpenseDto.GroupId, removeRestoreExpenseDto.ExpenseId);
    if(restoreExpenseRes.IsFailure) return Results.BadRequest(restoreExpenseRes.Error);

    var getGroupRes = await repo.GetGroupById(removeRestoreExpenseDto.GroupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());


  }
}