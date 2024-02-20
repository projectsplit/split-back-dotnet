using System.Security.Claims;
using SplitBackApi.Data.Repositories.ExpenseRepository;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints
{

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetLatestExpenses(
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    HttpRequest request
  )
  {
    var limitQuery = request.Query["limit"].ToString();
    if (string.IsNullOrEmpty(limitQuery)) return Results.BadRequest("limit query is missing");
    if (int.TryParse(limitQuery, out var limit) is false) return Results.BadRequest("invalid limit");

    var lastQuery = request.Query["last"].ToString();
    if (string.IsNullOrEmpty(lastQuery)) return Results.BadRequest("last query is missing");
    if (DateTime.TryParse(lastQuery, out var last) is false) return Results.BadRequest("invalid last");

    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("group query is missing");

    var expensesResult = await expenseRepository.GetPaginatedExpensesByGroupId(groupId, limit, last);
    if (expensesResult.IsFailure) return Results.BadRequest(expensesResult.Error);

    var expenses = expensesResult.Value;

    return Results.Ok(expenses);
  }
}