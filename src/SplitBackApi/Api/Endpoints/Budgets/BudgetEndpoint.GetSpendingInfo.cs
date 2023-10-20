
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

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> GetSpendingInfo(
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

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    string day = "1";
    string budgetCurrency = currencyFromReq;

    Enum.TryParse(budgetTypeString, true, out BudgetType budgetType);

    var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;

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

    var budgetMaybe = await budgetRepository.GetByUserId(authenticatedUserId);

    var response = new SpendingInfoResponse
    {
      BudgetSubmitted = budgetMaybe.HasValue,
      TotalAmountSpent = Math.Round(totalSpent, 2).ToString(),
      Currency = budgetCurrency,
    };
    return Results.Ok(response);
  }

}
