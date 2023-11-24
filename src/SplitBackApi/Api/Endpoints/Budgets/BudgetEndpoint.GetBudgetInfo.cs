
using System.Security.Claims;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Api.Endpoints.Budgets.Responses;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Api.Services;
using Microsoft.IdentityModel.Tokens;

using SplitBackApi.Data.Repositories.ExchangeRateRepository;
using SplitBackApi.Api.Extensions;


namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> GetBudgetInfo(
    IBudgetRepository budgetRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    HttpRequest request,
    BudgetService budgetService,
    IExchangeRateRepository exchangeRateRepository
  )
  {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var budgetMaybe = await budgetRepository.GetByUserId(authenticatedUserId);
    if (budgetMaybe.HasNoValue) return Results.Ok(new BudgetInfoResponse { BudgetSubmitted = false });

    var budget = budgetMaybe.Value;

    var day = budget.Day;
    var budgetType = budget.BudgetType;
    var budgetCurrency = budget.Currency;

    var dates = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value;
    var startDate = dates.startDate;
    var endDate = dates.endDate;

    var currentDate = DateTime.Now;

    var totalSpentResult = await budgetService.CalculateTotalSpentInSingleCurrency(
          authenticatedUserId,
          groups,
          budgetCurrency,
          startDate
         );

    if (totalSpentResult.IsFailure) return Results.BadRequest(totalSpentResult.Error);
    var totalSpent = totalSpentResult.Value;


    var remainingDaysResult = budgetService.RemainingDays(budgetType, startDate);
    if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

    var remainingDays = remainingDaysResult.Value;
    var daysSinceStartDay = (currentDate - startDate).Days;

    var averageSpentPerDay = (daysSinceStartDay == 0)
        ? Math.Round(totalSpent.Amount, 2)
        : Math.Round(totalSpent.Amount / daysSinceStartDay, 2);

    var response = new BudgetInfoResponse
    {
      BudgetSubmitted = true,
      AverageSpentPerDay = averageSpentPerDay.ToString(),
      RemainingDays = remainingDays.ToString(),
      TotalAmountSpent = Math.Round(totalSpent.Amount, 2).ToString(),
      Goal = budget.Amount,
      Currency = budget.Currency,
      BudgetType = budget.BudgetType,
      StartDate = startDate,
      EndDate = endDate
    };

    return Results.Ok(response);

  }

}
