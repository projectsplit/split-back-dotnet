
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

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> BudgetInfo(
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
    var budgetTypeString = request.Query["budgetType"].ToString();

    var currencyFromReq = request.Query["currency"];

    var groupsResult = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;

    decimal totalSpent = 0;
    string day;
    string budgetCurrency;
    BudgetType budgetType;

    var budgetResult = await budgetRepository.GetByUserId(authenticatedUserId);
    if (budgetResult.IsFailure)
    {
      day = "1";
      Enum.TryParse<BudgetType>(budgetTypeString, true, out budgetType);
      budgetCurrency = "USD";
    }
    else
    {
      day = budgetResult.Value.Day;
      budgetType = budgetResult.Value.BudgetType;
      budgetCurrency = budgetResult.Value.Currency;
    }

    var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;
    var endDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.endDate;
    var currentDate = DateTime.Now;

    foreach (var group in groups)
    {
      string groupId = group.Id;
      string memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expensesResult = await expenseRepository.GetWhereMemberIsParticipant(groupId, memberId, startDate);
      if (expensesResult.IsFailure) return Results.BadRequest(expensesResult.Error);

      var expenses = expensesResult.Value;

      foreach (var expense in expenses)
      {
        string currency = expense.Currency;
        decimal amount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();

        if (currency == budgetCurrency)
        {
          totalSpent += amount;
        }
        else
        {
          // var historicalFxRateResult = await budgetService.HistoricalFxRate(currency, budgetCurrency, expense.CreationTime.ToString("yyyy-MM-dd"));
          // if (historicalFxRateResult.IsFailure) return Results.BadRequest(historicalFxRateResult.Error);
          // var historicalFxRate = historicalFxRateResult.Value.Rates;
          // totalSpent += amount / historicalFxRate[expense.Currency];

        }
      }

    }
    if (budgetResult.IsFailure)
    {
      var response = new NoBudgetInfoResponse
      {
        BudgetSubmitted = false,
        TotalAmountSpent = Math.Round(totalSpent, 2).ToString(),
        Currency = budgetCurrency,
      };
      return Results.Ok(response);
    }
    else
    {
      decimal averageSpentPerDay;
      var budget = budgetResult.Value;
      var remainingDaysResult = budgetService.RemainingDays(budgetType, startDate);
      if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

      var remainingDays = remainingDaysResult.Value;
      var daysSinceStartDay = (currentDate - startDate).Days;

      averageSpentPerDay = (daysSinceStartDay == 0) ? Math.Round(totalSpent, 2) : Math.Round(totalSpent / daysSinceStartDay, 2);

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

}
