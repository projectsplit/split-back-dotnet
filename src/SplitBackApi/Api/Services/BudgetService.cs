using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.ExchangeRateRepository;
using NMoneys;
using SplitBackApi.Api.Helper;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Services;

public class BudgetService
{
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  public BudgetService(IExpenseRepository expenseRepository, ITransferRepository transferRepository, IExchangeRateRepository exchangeRateRepository)
  {
    _expenseRepository = expenseRepository;
    _transferRepository = transferRepository;
    _exchangeRateRepository = exchangeRateRepository;

  }
  public async Task<Result<decimal>> CalculateTotalSpent(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string budgetCurrency,
    DateTime startDate)
  {
    //TODO store as CurrencyIsoCode in DB?
    var budgetCurrencyISO = MoneyHelper.StringToIsoCode(budgetCurrency);

    var groupIdToMemberIdMap = MemberIdHelper.GroupIdsToMemberIdsMap(groups, authenticatedUserId);

    var expenses = await _expenseRepository.GetLatestByGroupsIdsMembersIdsAndStartDate(groupIdToMemberIdMap, startDate);
    var transfers = await _transferRepository.GetLatestByGroupsIdsMembersIdsAndStartDate(groupIdToMemberIdMap, startDate);

    var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, budgetCurrency);
    if (!exchangeRates.Any()) return Result.Failure<decimal>("exchange rates not in DB");

    var expensesTotalSpent = expenses.Any() ?
      Money.Total(expenses
      .SelectMany(e => e.Participants
      .Where(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])
      .Select(p => new Money(p.ParticipationAmount.ToDecimal() / exchangeRates[e.Id], budgetCurrencyISO))))
      : Money.Zero(budgetCurrencyISO);

    var transfersTotalSent = transfers.Any() ? Money.Total(transfers
      .Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId])
      .Select(t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
      : Money.Zero(budgetCurrencyISO);

    var transfersTotalReceived = transfers.Any() ? Money.Total(transfers
      .Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId])
      .Select(t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
      : Money.Zero(budgetCurrencyISO);

    var totalMoney = expensesTotalSpent.Plus(transfersTotalSent.Minus(transfersTotalReceived));
    return totalMoney.Amount;
  }

  private async Task<Dictionary<string, decimal>> GetAllRatesFromAllOperations(
     List<Expense> expenses,
     List<Transfer> transfers,
     string budgetCurrency
     )
  {
    var expenseDates = expenses.Select(e => e.ExpenseTime.ToString("yyyy-MM-dd"));
    var transferDates = transfers.Select(t => t.TransferTime.ToString("yyyy-MM-dd"));
    var allDates = expenseDates.Concat(transferDates).Distinct().ToList();

    var allRatesMaybe = await _exchangeRateRepository.GetAllRatesForDates(allDates);
    var allRates = allRatesMaybe.Value;

    var dateToExchangeRatesDictionary = allDates.ToDictionary(
    date => date,
    date => allRates.FirstOrDefault(rate => rate.Date == date));

    var validatedDateToExchangeRatesDictionary = await EnsureAllExchangeRates(dateToExchangeRatesDictionary);

    var expensesRates = expenses.Select(expense =>
    {
      var rate = CalculateExchangeRate(budgetCurrency, expense.Currency, validatedDateToExchangeRatesDictionary, expense.ExpenseTime.ToString("yyyy-MM-dd"));
      return new KeyValuePair<string, decimal>(expense.Id, rate);
    });

    var transfersRates = transfers.Select(transfer =>
   {
     var rate = CalculateExchangeRate(budgetCurrency, transfer.Currency, validatedDateToExchangeRatesDictionary, transfer.TransferTime.ToString("yyyy-MM-dd"));
     return new KeyValuePair<string, decimal>(transfer.Id, rate);
   });

    var rateDictionary = expensesRates
        .Concat(transfersRates)
        .ToDictionary(pair => pair.Key, pair => pair.Value);

    return rateDictionary;
  }

  private async Task<Dictionary<string, ExchangeRates>> EnsureAllExchangeRates(Dictionary<string, ExchangeRates> dateToExchangeRatesDictionary)
  {
    var missingDates = dateToExchangeRatesDictionary
      .Where(kv => kv.Value == null)
      .Select(kv => kv.Key)
      .ToList();

    if (missingDates.IsNullOrEmpty()) return dateToExchangeRatesDictionary;

    var missingDateExchangeRates = await Task.WhenAll(missingDates.Select(RefetchExchangeRates));

    missingDateExchangeRates.Where(r => r.IsSuccess).ToList().ForEach(r =>
    {
      dateToExchangeRatesDictionary[r.Value.Date] = r.Value;
    });

    return dateToExchangeRatesDictionary;
  }

  private static decimal CalculateExchangeRate(
    string fromCurrency,
    string toCurrency,
    Dictionary<string, ExchangeRates> dateToExchangeRatesDictionary,
    string date)
  {

    switch (fromCurrency)
    {
      case "USD":
        var rate = dateToExchangeRatesDictionary[date].Rates[toCurrency];
        return rate;

      default:
        var denominatorRate = dateToExchangeRatesDictionary[date].Rates[fromCurrency];
        var nominatorRate = dateToExchangeRatesDictionary[date].Rates[toCurrency];
        rate = nominatorRate / denominatorRate;
        return rate;
    }
  }

  private async Task<Result<ExchangeRates>> RefetchExchangeRates(string date)
  {
    await _exchangeRateRepository.GetExchangeRatesFromExternalProvider("USD", date);
    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesByDate(date);

    if (exchangeRates.IsFailure) return Result.Failure<ExchangeRates>($"Could not refetch exchange rates for {date}");

    return exchangeRates;
  }

  public Result<(DateTime startDate, DateTime endDate)> StartAndEndDateBasedOnBudgetAndDay(BudgetType budgetType, string day)
  {
    DateTime currentDate = DateTime.Now;
    DateTime startDate;
    DateTime endDate;
    var day2Int = day.ToInt();

    switch (budgetType)
    {
      case BudgetType.Monthly:

        if (currentDate.Day >= day2Int)
        {
          var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
          startDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(day2Int, daysInMonth));
        }
        else
        {
          // Calculate the previous month
          var previousMonth = currentDate.AddMonths(-1);

          if (currentDate.Month == 1) //check if January so it will go to Dec of prev year
          {
            // If we are in January, go back to December of the previous year
            var daysInMonth = DateTime.DaysInMonth(previousMonth.Year - 1, 12);
            startDate = new DateTime(previousMonth.Year - 1, 12, Math.Min(day2Int, daysInMonth));
          }
          else
          {
            // Go back to the previous month of the current year
            var daysInMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            startDate = new DateTime(previousMonth.Year, previousMonth.Month, Math.Min(day2Int, daysInMonth));
          }
        }

        var tempEndDate = startDate.AddMonths(1);
        //case where previous month has 30 and next month 31 days.
        //February does not have an issue as regardless at what day the previous month ends
        //adding a month to it will alwasy bring to the 28 or 29 of Feb.
        endDate = (tempEndDate.Day == 30 && day2Int == 31) ? tempEndDate.AddDays(1) : tempEndDate;

        if (endDate < currentDate)
        {
          startDate = endDate.Date;
          endDate = startDate.AddMonths(1);
        };

        break;

      case BudgetType.Weekly:
        var dayDifference = (int)currentDate.DayOfWeek - day2Int;

        if (dayDifference >= 0)
        {
          startDate = currentDate.AddDays(-dayDifference);
        }
        else
        {
          // Start from the day2Int of the previous week
          startDate = currentDate.AddDays(-dayDifference - 7);
        }
        startDate = startDate.Date;
        endDate = startDate.AddDays(7);
        break;


      default:
        throw new NotImplementedException("Unsupported budget type");
    }

    return Result.Success((startDate, endDate));
  }

  public Result<double> RemainingDays(BudgetType budgetType, DateTime startDate)
  {
    var currentDate = DateTime.Now;
    var remainingDays = budgetType switch
    {
      BudgetType.Monthly => (startDate.AddMonths(1) - currentDate).TotalDays,
      BudgetType.Weekly => (startDate.AddDays(7) - currentDate).TotalDays,
      _ => throw new NotImplementedException("Unsupported budget type"),
    };
    return Result.Success(remainingDays);

  }

}