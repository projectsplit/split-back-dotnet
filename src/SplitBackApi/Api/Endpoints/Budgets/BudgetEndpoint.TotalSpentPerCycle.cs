
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
  private static async Task<IResult> TotalSpentPerCycle(
    IBudgetRepository budgetRepository,
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal
  )
  {

    var authenticatedUserId = "63ff33b09e4437f07d9d3982"; //claimsPrincipal.GetAuthenticatedUserId();

    var groupsResult = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groupsResult.IsFailure) return Results.BadRequest(groupsResult.Error);

    var groups = groupsResult.Value;
   
    Dictionary<string, decimal> currencyAndAmount = new();

    foreach (var group in groups)
    {
      string groupId = group.Id;
      string memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expensesResult = await expenseRepository.GetWhereMemberIsParticipant(BudgetType.Weekly, groupId, memberId, "1");
      if (expensesResult.IsFailure) return Results.BadRequest(expensesResult.Error);

      var expenses = expensesResult.Value;

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


    return Results.Ok(currencyAndAmount);

  }

}