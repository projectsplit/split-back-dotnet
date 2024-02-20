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
using NMoneys;

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
    var authenticatedUserId ="63ff33b09e4437f07d9d3982";//claimsPrincipal.GetAuthenticatedUserId();

    var budgetTypeString = request.Query["budgetType"].ToString();
    if (string.IsNullOrEmpty(budgetTypeString) || !Enum.TryParse(budgetTypeString, out BudgetType budgetType))
      return Results.BadRequest("BudgetType is missing, empty or invalid.");

    var currencyFromReq = request.Query["currency"];
    if (string.IsNullOrEmpty(currencyFromReq) || !Enum.TryParse(currencyFromReq, out CurrencyIsoCode _)) Results.BadRequest("Currency is missing, empty or invalid.");

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    string day = "1";
    string budgetCurrency = currencyFromReq;


    var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;
    var currentDate = DateTime.Now;
    var totalSpentResult = await budgetService.CalculateTotalSpentInSingleCurrency(
          authenticatedUserId,
          groups,
          budgetCurrency,
          startDate,
          currentDate
         );

    if (totalSpentResult.IsFailure) return Results.BadRequest(totalSpentResult.Error);
    var totalSpent = totalSpentResult.Value;

    var budgetMaybe = await budgetRepository.GetByUserId(authenticatedUserId);

    var response = new SpendingInfoResponse
    {
      BudgetSubmitted = budgetMaybe.HasValue,
      TotalAmountSpent = Math.Round(totalSpent.Amount, 2).ToString(),
      Currency = budgetCurrency,
    };
    return Results.Ok(response);
  }

}
