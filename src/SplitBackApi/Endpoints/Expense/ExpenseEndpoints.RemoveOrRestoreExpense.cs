using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> RemoveOrRestoreExpense(IRepository repo,RemoveRestoreExpenseDto removeRestoreExpenseDto)
    {
        try
      {//need transaction?
        var groupId = ObjectId.Parse(removeRestoreExpenseDto.GroupId);
        await repo.RemoveOrRestoreExpense(removeRestoreExpenseDto);
        var group = await repo.GetGroupById(groupId);
        if (group is null) throw new Exception();
        return Results.Ok(group.PendingTransactions());
      }
      catch (Exception ex)
      {
        return Results.BadRequest(ex.Message);
      }
    }
}