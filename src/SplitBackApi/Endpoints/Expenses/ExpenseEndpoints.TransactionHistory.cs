using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> TransactionHistory(
    IRepository repo,
    TransactionHistoryDto transactionHistoryDto,
    HttpContext context) {
    
    var authenticatedUserIdResult = context.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var result = await repo.GetGroupById(transactionHistoryDto.GroupId);

    if(result.IsFailure) return Results.BadRequest(result.Error);

    var group = result.Value;
    if(group.Expenses.Count == 0) {
      return Results.Ok(new List<TransactionTimelineItem>());
    }

    return Results.Ok(group.GetTransactionHistory(authenticatedUserId));
  }
}