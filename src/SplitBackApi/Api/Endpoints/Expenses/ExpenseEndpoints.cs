namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  public static void MapExpenseEndpoints(this IEndpointRouteBuilder app) {

    var expenseGroup = app.MapGroup("/expense")
      .WithTags("Expenses").AllowAnonymous();

    expenseGroup.MapPost("/create", CreateExpense);
    expenseGroup.MapPost("/edit", EditExpense);
    expenseGroup.MapPost("/remove", RemoveExpense);
    expenseGroup.MapPost("/getgroupexpenses", GetExpensesByGroup);
    expenseGroup.MapGet("/latest-expenses", GetLatestExpenses);
  }
}