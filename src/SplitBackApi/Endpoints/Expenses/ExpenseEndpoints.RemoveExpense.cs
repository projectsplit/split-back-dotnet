using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> RemoveExpense(IRepository repo, RemoveRestoreExpenseDto removeRestoreExpenseDto) {
    var groupId = ObjectId.Parse(removeRestoreExpenseDto.GroupId);

    var removeExpenseRes = await repo.RemoveExpense(removeRestoreExpenseDto.GroupId, removeRestoreExpenseDto.ExpenseId);
    if(removeExpenseRes.IsFailure) return Results.BadRequest(removeExpenseRes.Error);
    
    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());

  }
}