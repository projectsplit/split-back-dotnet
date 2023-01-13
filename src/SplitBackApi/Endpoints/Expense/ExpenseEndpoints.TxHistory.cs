using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> TxHistory(IRepository repo, TransactionHistoryDto txHistoryDto)
    {
        var groupId = ObjectId.Parse(txHistoryDto.GroupId);
        try
        {
            var group = await repo.GetGroupById(groupId);
            if (group is null) throw new Exception();
            if (group.Expenses.Count == 0)
            {
                return Results.Ok(new List<TransactionTimelineItem>());
            }
            return Results.Ok(group.GetTransactionHistory());
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.InnerException);
        }
    }
}