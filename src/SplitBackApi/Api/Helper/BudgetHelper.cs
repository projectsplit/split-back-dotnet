using CSharpFunctionalExtensions;
using SplitBackApi.Data.Repositories.ExchangeRateRepository;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using NMoneys;

namespace SplitBackApi.Api.Helper;

public static class BudgetHelper
{
  public static async Task<Result<decimal>> CalculateTotalSpent(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string budgetCurrency,
    DateTime startDate,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IExchangeRateRepository exchangeRateRepository)
  {
    var budgetCurrencyISO = Enum.Parse<CurrencyIsoCode>(budgetCurrency);
    var totalSpentPerGroup = await Task.WhenAll(groups.Select(async group =>
    {
      var groupId = group.Id;
      var memberId = UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId);

      var expenses = await expenseRepository.GetLatestByGroupIdMemberId(groupId, memberId, startDate);

      var transfers = await transferRepository.GetByGroupIdAndStartDate(groupId, memberId, startDate);

      var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, budgetCurrency, exchangeRateRepository);

      var expensesTotalSpent = expenses.Any()
        ? Money.Total(expenses.Select(e => new Money(e.Amount.ToDecimal() / exchangeRates[e.Id], budgetCurrencyISO)))
        : Money.Zero(budgetCurrencyISO);

      var transfersTotalSent = transfers.Any() ? Money.Total(transfers
        .Where(t => t.SenderId == memberId)
        .Select(t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
        :Money.Zero(budgetCurrencyISO);

      var transfersTotalReceived = transfers.Any() ? Money.Total(transfers
        .Where(t => t.ReceiverId == memberId)
        .Select(t => new Money((-1) * t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
        :Money.Zero(budgetCurrencyISO);

      return expensesTotalSpent.Plus(transfersTotalSent.Plus(transfersTotalReceived));
    }));

    var totalSpent = Money.Total(totalSpentPerGroup);

    return totalSpent.Amount;
  }

  private static async Task<Dictionary<string, decimal>> GetAllRatesFromAllOperations(
     List<Expense> expenses,
     List<Transfer> transfers,
     string budgetCurrency,
     IExchangeRateRepository exchangeRateRepository)
  {
    var expensesRateResults = await Task.WhenAll(expenses.Select(async expense =>
    {
      var rateResult = await exchangeRateRepository.GetRate(budgetCurrency, expense.Currency, expense.ExpenseTime.ToString("yyyy-MM-dd"));
      if (rateResult.IsFailure) return Result.Failure<KeyValuePair<string, decimal>>($"could not retrieve rate for {budgetCurrency}/{expense.Currency}");

      var rate = rateResult.Value;
      return new KeyValuePair<string, decimal>(expense.Id, rate);
    }));

    var transfersRateResults = await Task.WhenAll(transfers.Select(async transfer =>
   {
     var rateResult = await exchangeRateRepository.GetRate(budgetCurrency, transfer.Currency, transfer.TransferTime.ToString("yyyy-MM-dd"));
     if (rateResult.IsFailure) return Result.Failure<KeyValuePair<string, decimal>>($"could not retrieve rate for {budgetCurrency}/{transfer.Currency}");

     var rate = rateResult.Value;
     return new KeyValuePair<string, decimal>(transfer.Id, rate);
   }));


    var rateDictionary = expensesRateResults
        .Where(result => result.IsSuccess)
        .Concat(transfersRateResults
        .Where(result => result.IsSuccess))
        .Select(result => result.Value)
        .ToDictionary(pair => pair.Key, pair => pair.Value);

    return rateDictionary;
  }
}



//     string[] dateArray = {
//     "2023-09-23", "2023-09-24", "2023-09-25", "2023-09-26", "2023-09-27",
//     "2023-09-28", "2023-09-29", "2023-09-30", "2023-10-01", "2023-10-02",
//     "2023-10-03", "2023-10-04", "2023-10-05", "2023-10-06", "2023-10-07",
//     "2023-10-08", "2023-10-09", "2023-10-10", "2023-10-11", "2023-10-12",
//     "2023-10-13", "2023-10-14", "2023-10-15", "2023-10-16", "2023-10-17",
//     "2023-10-18", "2023-10-19", "2023-10-20", "2023-10-21", "2023-10-22",
//     "2023-10-23"
// };
//     var tasks = dateArray.Select(async d =>
//     {
//       await exchangeRateRepository.GetExchangeRates("USD", d);
//     });

//     await Task.WhenAll(tasks);