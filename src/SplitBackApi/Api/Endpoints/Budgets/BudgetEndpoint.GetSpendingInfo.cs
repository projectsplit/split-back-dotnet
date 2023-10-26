
using System.Security.Claims;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Api.Endpoints.Budgets.Responses;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Api.Services;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Data.Repositories.ExchangeRateRepository;
using SplitBackApi.Api.Extensions;

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
     BudgetService budgetService,
     IExchangeRateRepository exchangeRateRepository
   )
  {
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var budgetTypeString = request.Query["budgetType"].ToString();
    if (string.IsNullOrEmpty(budgetTypeString)) return Results.BadRequest("BudgetType is missing or empty.");

    var currencyFromReq = request.Query["currency"];
    if (string.IsNullOrEmpty(currencyFromReq)) Results.BadRequest("Currency is missing or empty.");

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    string day = "1";
    string budgetCurrency = currencyFromReq;

    Enum.TryParse(budgetTypeString, true, out BudgetType budgetType);

    var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;

    var totalSpentResult = await budgetService.CalculateTotalSpent(
          authenticatedUserId,
          groups,
          budgetCurrency,
          startDate
         );

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
