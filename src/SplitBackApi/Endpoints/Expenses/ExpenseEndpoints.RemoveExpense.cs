using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> RemoveExpense(IRepository repo,RemoveRestoreExpenseDto removeRestoreExpenseDto)
    {
        var groupId = ObjectId.Parse(removeRestoreExpenseDto.GroupId);
        await repo.RemoveExpense(removeRestoreExpenseDto.GroupId,removeRestoreExpenseDto.ExpenseId);
        var result = await repo.GetGroupById(groupId);
       
        return result.Match(group => {
        return Results.Ok(group.PendingTransactions());
      }, e => {
        return Results.BadRequest(e.Message);
      });
    }
}