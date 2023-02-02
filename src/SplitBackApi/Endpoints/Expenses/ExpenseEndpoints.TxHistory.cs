using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using MongoDB.Bson;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> TxHistory(
    IRepository repo,
    TransactionHistoryDto txHistoryDto,
    HttpContext ctx) {

    var groupId = ObjectId.Parse(txHistoryDto.GroupId);
    var authedUserId = ctx.GetAuthorizedUserId();

    var result = await repo.GetGroupById(groupId);

    if(result.IsFailure) return Results.BadRequest(result.Error);

    var group = result.Value;
    if(group.Expenses.Count == 0) {
      return Results.Ok(new List<TransactionTimelineItem>());
    }

    return Results.Ok(group.GetTransactionHistory(authedUserId));
  }
}