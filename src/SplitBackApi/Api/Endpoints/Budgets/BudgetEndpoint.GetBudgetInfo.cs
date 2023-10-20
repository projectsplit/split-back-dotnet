
using System.Security.Claims;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Api.Endpoints.Budgets.Responses;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Api.Services;
using Microsoft.IdentityModel.Tokens;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<Microsoft.AspNetCore.Http.IResult> GetBudgetInfo(
    IBudgetRepository budgetRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    HttpRequest request,
    BudgetService budgetService
  )
  {

    var authenticatedUserId = "63ff33b09e4437f07d9d3982"; //claimsPrincipal.GetAuthenticatedUserId();

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var budgetMaybe = await budgetRepository.GetByUserId(authenticatedUserId);
    if (budgetMaybe.HasNoValue) return Results.Ok(new BudgetInfoResponse { BudgetSubmitted = false });

    var budget = budgetMaybe.Value;

    var day = budget.Day;
    var budgetType = budget.BudgetType;
    var budgetCurrency = budget.Currency;

    var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;
    var endDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.endDate;
    var currentDate = DateTime.Now;

    var totalSpentResult = await BudgetHelper.CalculateTotalSpent(
          authenticatedUserId,
          groups,
          budgetCurrency,
          startDate,
          expenseRepository,
          transferRepository,
          budgetService);

    if (totalSpentResult.IsFailure) return Results.BadRequest(totalSpentResult.Error);
    var totalSpent = totalSpentResult.Value;


    var remainingDaysResult = budgetService.RemainingDays(budgetType, startDate);
    if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

    var remainingDays = remainingDaysResult.Value;
    var daysSinceStartDay = (currentDate - startDate).Days;

    var averageSpentPerDay = (daysSinceStartDay == 0)
        ? Math.Round(totalSpent, 2)
        : Math.Round(totalSpent / daysSinceStartDay, 2);

    var response = new BudgetInfoResponse
    {
      BudgetSubmitted = true,
      AverageSpentPerDay = averageSpentPerDay.ToString(),
      RemainingDays = remainingDays.ToString(),
      TotalAmountSpent = Math.Round(totalSpent, 2).ToString(),
      Goal = budget.Amount,
      Currency = budget.Currency,
      BudgetType = budget.BudgetType,
      StartDate = startDate,
      EndDate = endDate
    };

    return Results.Ok(response);

  }

}
