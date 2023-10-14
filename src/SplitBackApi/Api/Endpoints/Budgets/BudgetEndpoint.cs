namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{

  public static void MapBudgetEndpoints(this IEndpointRouteBuilder app)
  {

    var budgetGroup = app.MapGroup("/budget")
      .WithTags("Budget").AllowAnonymous();

    budgetGroup.MapPost("/create", CreateBudget);
    budgetGroup.MapPost("/delete", DeleteBudget);
    budgetGroup.MapGet("/budgetinfo", GetBudgetInfo);
  }
}