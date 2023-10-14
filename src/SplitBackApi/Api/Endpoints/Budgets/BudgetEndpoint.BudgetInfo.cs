using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Api.Endpoints.Budgets.Responses;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Api.Services;

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
        BudgetService budgetService)
    {
        // var authenticatedUserId = "63ff33b09e4437f07d9d3982";
        var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

        // TODO Handle no budgetType
        var budgetTypeString = request.Query["budgetType"].ToString();

        // TODO Handle no currency
        var currencyFromReq = request.Query["currency"];

        var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
        if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

        decimal totalSpent = 0;
        string day;
        string budgetCurrency;
        BudgetType budgetType;

        var budgetMaybe = await budgetRepository.GetByUserId(authenticatedUserId);
        if (budgetMaybe.HasNoValue)
        {
            day = "1";
            Enum.TryParse<BudgetType>(budgetTypeString, true, out budgetType);
            budgetCurrency = "USD";
        }
        else
        {
            day = budgetMaybe.Value.Day;
            budgetType = budgetMaybe.Value.BudgetType;
            budgetCurrency = budgetMaybe.Value.Currency;
        }

        var startDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.startDate;
        var endDate = budgetService.StartAndEndDateBasedOnBudgetAndDay(budgetType, day).Value.endDate;
        var currentDate = DateTime.Now;

        foreach (var group in groups)
        {
            var groupId = group.Id;
            var memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

            var expenses = await expenseRepository.GetLatestByGroupIdMemberId(groupId, memberId, startDate);
            if (expenses.IsNullOrEmpty()) return Results.BadRequest("No expenses found");

            var transfers = await transferRepository.GetByGroupIdAndStartDate(groupId, memberId, startDate);
            if (transfers.IsNullOrEmpty()) return Results.BadRequest("No transfers found");

            foreach (var expense in expenses)
            {
                var currency = expense.Currency;
                var amount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();

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

            foreach (var transfer in transfers)
            {
                string currency = transfer.Currency;
                decimal amount = transfer.Amount.ToDecimal();

                if (currency == budgetCurrency)
                {
                    totalSpent = transfer.SenderId == memberId ? totalSpent + amount : totalSpent - amount;
                }
                else
                {
                    // var historicalFxRateResult = await budgetService.HistoricalFxRate(currency, budgetCurrency, transfer.CreationTime.ToString("yyyy-MM-dd"));
                    // if (historicalFxRateResult.IsFailure) return Results.BadRequest(historicalFxRateResult.Error);
                    // var historicalFxRate = historicalFxRateResult.Value.Rates;

                    // totalSpent = transfer.SenderId == memberId ?
                    // totalSpent + amount / historicalFxRate[transfer.Currency] :
                    // totalSpent - amount / historicalFxRate[transfer.Currency];
                }
            }
        }

        if (budgetMaybe.HasNoValue)
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
            var budget = budgetMaybe.Value;
            var remainingDaysResult = budgetService.RemainingDays(budgetType, startDate);
            if (remainingDaysResult.IsFailure) return Results.BadRequest(remainingDaysResult.Error);

            var remainingDays = remainingDaysResult.Value;
            var daysSinceStartDay = (currentDate - startDate).Days;

            averageSpentPerDay = (daysSinceStartDay == 0)
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
}