using CSharpFunctionalExtensions;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Helper;

public class BudgetHelper
{
  // private static decimal GetRate(string currencyFrom, string currencyTo, DateTime timestamp)
  // {
  //   return 0.9m;
  // }

  public static async Task<Result<decimal>> CalculateTotalSpent(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string budgetCurrency,
    DateTime startDate,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    BudgetService budgetService)
  {
    decimal totalSpent = 0;

    foreach (var group in groups)
    {
      var groupId = group.Id;
      var memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expenses = await expenseRepository.GetLatestByGroupIdMemberId(groupId, memberId, startDate);

      var transfers = await transferRepository.GetByGroupIdAndStartDate(groupId, memberId, startDate);

      // var expensesTotalSpent = expenses
      //   .Sum(e => e.Amount.ToDecimal() / GetRate(e.Currency, budgetCurrency, e.CreationTime));

      // var transfersTotalSent = transfers
      //   .Where(t => t.SenderId == memberId)
      //   .Sum(t => t.Amount.ToDecimal() / GetRate(t.Currency, budgetCurrency, t.CreationTime));

      // var transfersTotalReceived = transfers
      //   .Where(t => t.ReceiverId == memberId)
      //   .Sum(t => (-1) * t.Amount.ToDecimal() / GetRate(t.Currency, budgetCurrency, t.CreationTime));

      // return expensesTotalSpent + transfersTotalSent + transfersTotalReceived;


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
          // if (historicalFxRateResult.IsFailure) return Result.Failure<decimal>(historicalFxRateResult.Error);

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
          // if (historicalFxRateResult.IsFailure) return Result.Failure<decimal>(historicalFxRateResult.Error);
          // var historicalFxRate = historicalFxRateResult.Value.Rates;

          // totalSpent = transfer.SenderId == memberId ?
          // totalSpent + amount / historicalFxRate[transfer.Currency] :
          // totalSpent - amount / historicalFxRate[transfer.Currency];
        }
      }

    }
    return totalSpent;
  }
}