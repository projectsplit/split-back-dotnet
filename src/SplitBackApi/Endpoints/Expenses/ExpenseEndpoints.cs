namespace SplitBackApi.Endpoints;
public static partial class ExpenseEndpoints {
  public static void MapExpenseEndpoints(this IEndpointRouteBuilder app) {
    var expenseGroup = app.MapGroup("/expense")
      .WithTags("Expenses")
      .AllowAnonymous();
    expenseGroup.MapPost("/addExpense", AddExpense);
    expenseGroup.MapPost("/addComment", AddComment);
    expenseGroup.MapPost("/editExpense", EditExpense);
    expenseGroup.MapPost("/removeExpense", RemoveExpense);
    expenseGroup.MapPost("/restoreExpense", RestoreExpense);
    expenseGroup.MapPost("/txHistory", TxHistory);
  }
}