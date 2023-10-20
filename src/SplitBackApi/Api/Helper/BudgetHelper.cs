using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.TransferRepository;

using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Helper;

public class BudgetHelper
{

  public static async Task<(decimal TotalSpent, string ErrorMessage)> CalculateTotalSpent(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string budgetCurrency,
    DateTime startDate,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    BudgetService budgetService
    )
  {

    decimal totalSpent = 0;
    string errorMessage = null;

    foreach (var group in groups)
    {
      var groupId = group.Id;
      var memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId).Value;

      var expenses = await expenseRepository.GetLatestByGroupIdMemberId(groupId, memberId, startDate);
      if (expenses.IsNullOrEmpty())
      {
        errorMessage = "No expenses";
        break;
      }

      var transfers = await transferRepository.GetByGroupIdAndStartDate(groupId, memberId, startDate);
      if (transfers.IsNullOrEmpty())
      {
        errorMessage = "No transfers";
        break;
      }

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
          // if (historicalFxRateResult.IsFailure)
          // {
          //   errorMessage = historicalFxRateResult.Error;
          //   break;
          // }
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
          // if (historicalFxRateResult.IsFailure)
          // {
          //   errorMessage = historicalFxRateResult.Error;
          //   break;
          // }
          // var historicalFxRate = historicalFxRateResult.Value.Rates;

          // totalSpent = transfer.SenderId == memberId ?
          // totalSpent + amount / historicalFxRate[transfer.Currency] :
          // totalSpent - amount / historicalFxRate[transfer.Currency];
        }
      }

    }
    return (totalSpent, errorMessage);
  }


}