
using System.Security.Claims;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Extensions;
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
    // var budgetTypeString = request.Query["budgetType"].ToString();

    // if (Enum.TryParse(budgetTypeString, out BudgetType budgetType))
    // {
    var groupsResult = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;

    Dictionary<string, decimal> currencyAndAmount = new();

    var budgetResult = await budgetRepository.GetByUserId(authenticatedUserId);
    string day = budgetResult.IsFailure ? "1" : budgetResult.Value.Day; //calculate budget by taking first day of current week or month as a starting point
    var startDate = BudgetHelpers.StartDateBasedOnBudgetAndDay(budgetResult.Value.BudgetType, day).Value;
    var currentDate = DateTime.Now;
    List<Expense> expenses = new();

    foreach (var group in groups)
    {
      string groupId = group.Id;
      string memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expensesResult = await expenseRepository.GetWhereMemberIsParticipant(budgetResult.Value.BudgetType, groupId, memberId, startDate);
      if (expensesResult.IsFailure) return Results.BadRequest(expensesResult.Error);

      expenses.AddRange(expensesResult.Value);

      foreach (var expense in expenses)
      {
        string currency = expense.Currency;
        decimal amount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();

        if (currencyAndAmount.ContainsKey(currency))
        {
          currencyAndAmount[currency] += amount;
        }
        else
        {
          currencyAndAmount[currency] = amount;
        }
      }
    }

    var remainingDays = BudgetHelpers.RemainingDays(budgetResult.Value.BudgetType, startDate);

    return Results.Ok(currencyAndAmount);
  }

  // else
  // {
  //   return Results.BadRequest("Could not parse budget type");
  // }
  // (
  //      startDate.Month < currentDate.Month && startDate.Year == currentDate.Year
  //   || startDate.Year < currentDate.Year && startDate.Month > currentDate.Month)

}
