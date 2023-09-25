
using System.Security.Claims;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Api.Endpoints.Budgets.Responses;
namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> BudgetInfo(
    IBudgetRepository budgetRepository,
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    HttpRequest request
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

    var startDate = BudgetHelpers.StartDateBasedOnBudgetAndDay(budgetType, day).Value;
    var currentDate = DateTime.Now;

    foreach (var group in groups)
    {
      string groupId = group.Id;
      string memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expensesResult = await expenseRepository.GetWhereMemberIsParticipant(budgetType, groupId, memberId, startDate);
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
          // var historicalFxRateResult = await BudgetHelpers.HistoricalFxRate(currency, budgetCurrency, expense.CreationTime.ToString("yyyy-MM-dd"));
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
      var remainingDaysResult = BudgetHelpers.RemainingDays(budgetType, startDate);
      if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

      var remainingDays = remainingDaysResult.Value;
      var daysSinceStartDay = (currentDate - startDate).Days;

      if (daysSinceStartDay == 0)
      {
        averageSpentPerDay = Math.Round(totalSpent, 2);
      }
      else
      {
        averageSpentPerDay = Math.Round(totalSpent / daysSinceStartDay, 2);
      }

      var response = new BudgetInfoResponse
      {
        BudgetSubmitted = true,
        AverageSpentPerDay = averageSpentPerDay.ToString(),
        RemainingDays = Math.Round(remainingDays, 1).ToString(),
        TotalAmountSpent = Math.Round(totalSpent, 2).ToString(),
        Goal = budget.Amount,
        Currency = budget.Currency,
        BudgetType = budget.BudgetType,
        Day = budget.Day
      };

      return Results.Ok(response);
    }
  }

  // else
  // {
  //   return Results.BadRequest("Could not parse budget type");
  // }
  // (
  //      startDate.Month < currentDate.Month && startDate.Year == currentDate.Year
  //   || startDate.Year < currentDate.Year && startDate.Month > currentDate.Month)

}
