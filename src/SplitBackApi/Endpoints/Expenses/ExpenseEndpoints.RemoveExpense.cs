using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  
  private static async Task<IResult> RemoveExpense(IRepository repo, RemoveRestoreExpenseDto removeRestoreExpenseDto) {
    
    var removeExpenseResult = await repo.RemoveExpense(removeRestoreExpenseDto.GroupId, removeRestoreExpenseDto.ExpenseId);
    if(removeExpenseResult.IsFailure) return Results.BadRequest(removeExpenseResult.Error);
    
    var getGroupResult = await repo.GetGroupById(removeRestoreExpenseDto.GroupId);
    if(getGroupResult.IsFailure) return Results.BadRequest(getGroupResult.Error);
    var group = getGroupResult.Value;
    
    return Results.Ok(group.PendingTransactions());

  }
}