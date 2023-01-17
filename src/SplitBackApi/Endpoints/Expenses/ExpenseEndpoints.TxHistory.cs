using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> TxHistory(IRepository repo, TransactionHistoryDto txHistoryDto) {
    var groupId = ObjectId.Parse(txHistoryDto.GroupId);

    var result = await repo.GetGroupById(groupId);

    //var x = result.IfFail(e => Results.BadRequest(e.Message));
      return result.Match(group => {

        if(group.Expenses.Count == 0) {
          return Results.Ok(new List<TransactionTimelineItem>());
        }
        return Results.Ok(group.GetTransactionHistory());

      }, e => {
        return Results.BadRequest(e.Message);
      });

  }
}