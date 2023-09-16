
using System.Security.Claims;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Extensions;
using Newtonsoft.Json;
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
    //var budgetTypeString = request.Query["budgetType"].ToString();


    var groupsResult = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;


    decimal totalSpent = 0;
    var budgetResult = await budgetRepository.GetByUserId(authenticatedUserId);
    string day = budgetResult.IsFailure ? "1" : budgetResult.Value.Day; //calculate budget by taking first day of current week or month as a starting point
    var budget = budgetResult.Value;
    var startDate = BudgetHelpers.StartDateBasedOnBudgetAndDay(budget.BudgetType, day).Value;
    var currentDate = DateTime.Now;


    foreach (var group in groups)
    {
      string groupId = group.Id;
      string memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expensesResult = await expenseRepository.GetWhereMemberIsParticipant(budget.BudgetType, groupId, memberId, startDate);
      if (expensesResult.IsFailure) return Results.BadRequest(expensesResult.Error);

      var expenses = expensesResult.Value;

      foreach (var expense in expenses)
      {
        string currency = expense.Currency;
        decimal amount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();

        if (currency == budget.Currency)
        {
          totalSpent += amount;
        }
        else
        {
          // var historicalFxRateResult = await BudgetHelpers.HistoricalFxRate(currency, budget.Currency, expense.CreationTime.ToString("yyyy-MM-dd"));
          // if (historicalFxRateResult.IsFailure) return Results.BadRequest(historicalFxRateResult.Error);
          // var historicalFxRate = historicalFxRateResult.Value.Rates;
          // totalSpent += amount / historicalFxRate[expense.Currency];

        }
      }

    }

    var remainingDaysResult = BudgetHelpers.RemainingDays(budget.BudgetType, startDate);
    if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

    var remainingDays = remainingDaysResult.Value;
    var daysSinceStartDay = (currentDate - startDate).Days;
    var averageSpentPerDay = Math.Round(totalSpent / daysSinceStartDay, 2);

    var response = new BudgetInfoResponse
    {
      AverageSpentPerDay = averageSpentPerDay.ToString(),
      RemainingDays = remainingDays.ToString(),
      TotalAmountSpent = Math.Round(totalSpent, 2).ToString(),
      Goal = budget.Amount,
      Currency = budget.Currency,
      BudgetType = budget.BudgetType,
      Day = budget.Day
    };

    return Results.Ok(response);
  }

  // else
  // {
  //   return Results.BadRequest("Could not parse budget type");
  // }
  // (
  //      startDate.Month < currentDate.Month && startDate.Year == currentDate.Year
  //   || startDate.Year < currentDate.Year && startDate.Month > currentDate.Month)

}
