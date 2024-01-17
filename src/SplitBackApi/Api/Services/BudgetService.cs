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
using SplitBackApi.Api.Extensions;

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
  public async Task<Result<Money>> CalculateTotalSpentInSingleCurrency(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string budgetCurrency,
    DateTime startDate,
    DateTime endDate)
  {
    //TODO store as CurrencyIsoCode in DB?
    var budgetCurrencyISO = budgetCurrency.StringToIsoCode();

    var groupIdToMemberIdMap = MemberIdHelper.GroupIdsToMemberIdsMap(groups, authenticatedUserId);

    var expenses = await _expenseRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);
    var transfers = await _transferRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);
    if (expenses.Count == 0 && transfers.Count == 0) return new Money(0, budgetCurrencyISO);

    var transfersUserIsSender = transfers.Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId]).ToList();
    var transfersUserIsReceiver = transfers.Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId]).ToList();

    var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, budgetCurrency);
    if (!exchangeRates.Any()) return Result.Failure<Money>("exchange rates not in DB");

    var expensesTotalSpent = expenses.Any() ?
      Money.Total(expenses
      .SelectMany(e => e.Participants
      .Where(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])
      .Select(p => new Money(p.ParticipationAmount.ToDecimal() / exchangeRates[e.Id], budgetCurrencyISO))))
      : Money.Zero(budgetCurrencyISO);

    var transfersTotalSent = transfersUserIsSender.Any() ? Money.Total(transfers
      .Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId])
      .Select(t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
      : Money.Zero(budgetCurrencyISO);

    var transfersTotalReceived = transfersUserIsReceiver.Any() ? Money.Total(transfers
      .Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId])
      .Select(t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], budgetCurrencyISO)))
      : Money.Zero(budgetCurrencyISO);

    var totalMoney = expensesTotalSpent.Plus(transfersTotalSent.Minus(transfersTotalReceived));
    return totalMoney;
  }

  public async Task<Result<List<Money>>> CalculateCumulativeTotalSpentArray(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string requestedCurrency,
    DateTime startDate,
    DateTime endDate)
  {
    endDate = endDate > DateTime.Now ? DateTime.Now : endDate;
    var requestedCurrencyISO = requestedCurrency.StringToIsoCode();

    var groupIdToMemberIdMap = MemberIdHelper.GroupIdsToMemberIdsMap(groups, authenticatedUserId);

    var expenses = await _expenseRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);
    var transfers = await _transferRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);
    if (expenses.Count == 0 && transfers.Count == 0) return new List<Money>();

    var expensesUserIsParticipant = expenses.Where(e=>e.Participants.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])).ToList();
    var transfersUserIsSender = transfers.Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId]).ToList();
    var transfersUserIsReceiver = transfers.Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId]).ToList();

    var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, requestedCurrency);
    if (!exchangeRates.Any()) return Result.Failure<List<Money>>("exchange rates not in DB");

    var expensesDictionaryWithDatesSingleCurrency =
     expensesUserIsParticipant
     .ToDictionary(
      e => e.ExpenseTime,
      e => e.Participants
      .Where(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])
      .Select(p => new Money(p.ParticipationAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO))
      .FirstOrDefault()
     );

    var transfersUserIsSenderDictionaryWithDatesSingleCurrency =
      transfersUserIsSender.Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId])
      .ToDictionary(
      t => t.TransferTime,
      t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], requestedCurrencyISO));

    var transfersUserIsReceiverDictionaryWithDatesSingleCurrency =
      transfersUserIsReceiver.Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId])
      .ToDictionary(
      t => t.TransferTime,
      t => new Money(-t.Amount.ToDecimal() / exchangeRates[t.Id], requestedCurrencyISO));

    var dateRange = Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1)
      .Select(offset => startDate.AddDays(offset).Date);

    // Create a dictionary with all dates initialized to Money(0, requestedCurrencyISO)
    var defaultValues = dateRange.ToDictionary(date => date, _ => new Money(0, requestedCurrencyISO));

    var mergedDictionary = defaultValues
      .Concat(expensesDictionaryWithDatesSingleCurrency)
      .Concat(transfersUserIsSenderDictionaryWithDatesSingleCurrency
      .Concat(transfersUserIsReceiverDictionaryWithDatesSingleCurrency))
      .GroupBy(kvp => kvp.Key.Date)  // Group by date without time
      .Select(groupedItem =>
    {
      var totalAmount = Money.Total(groupedItem.Select(g => g.Value));
      return new KeyValuePair<DateTime, Money>(groupedItem.Key, totalAmount);
    })
    .OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    var cumulativeArray = mergedDictionary
      .Aggregate(new List<Money>(), (acc, kvp) =>
      {
        var previousTotal = acc.Count > 0 ? acc.Last() : Money.Zero(requestedCurrencyISO);
        var currentTotal = previousTotal + kvp.Value;
        acc.Add(currentTotal);
        return acc;
      });

    return Result.Success(cumulativeArray);
  }

  public async Task<Result<(List<Money> TotalLent, List<Money> TotalBorrowed)>> CalculateCumulativeTotalLentAndBorrowedArray(
    string authenticatedUserId,
    IEnumerable<Group> groups,
    string requestedCurrency,
    DateTime startDate,
    DateTime endDate)
  {
    endDate = endDate > DateTime.Now ? DateTime.Now : endDate;
    var requestedCurrencyISO = requestedCurrency.StringToIsoCode();

    var groupIdToMemberIdMap = MemberIdHelper.GroupIdsToMemberIdsMap(groups, authenticatedUserId);

    var expenses = await _expenseRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);

    var transfers = await _transferRepository.GetLatestByGroupsIdsMembersIdsStartDateEndDate(groupIdToMemberIdMap, startDate, endDate);
    if (expenses.Count == 0 && transfers.Count == 0) return (new List<Money>(),new List<Money>());

    var transfersUserIsSender = transfers.Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId]).ToList();
    var transfersUserIsReceiver = transfers.Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId]).ToList();

    var expensesUserIsLender = expenses
        .Where(e =>
            (e.Payers.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId] &&
            e.Participants.Any(part => part.MemberId == groupIdToMemberIdMap[e.GroupId]))
            &&
            (
             new Money(e.Payers.First(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).PaymentAmount.ToDecimal(), requestedCurrencyISO) >
             new Money(e.Participants.First(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).ParticipationAmount.ToDecimal(), requestedCurrencyISO)
            ))
            ||
            (
             e.Payers.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]) &&
            !e.Participants.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])
            )
        ).ToList();

      var expensesUserIsBorrower = expenses
          .Where(e =>
          (e.Payers.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId] &&
          e.Participants.Any(part => part.MemberId == groupIdToMemberIdMap[e.GroupId]))
          &&
          (
            new Money(e.Payers.First(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).PaymentAmount.ToDecimal(), requestedCurrencyISO) <
            new Money(e.Participants.First(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).ParticipationAmount.ToDecimal(), requestedCurrencyISO)
          ))
          ||
          (
          !e.Payers.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]) &&
          e.Participants.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId])
          )
      ).ToList();

    var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, requestedCurrency);
    if (!exchangeRates.Any()) return Result.Failure<(List<Money>,List<Money>)>("exchange rates not in DB");

    var expensesUserIsLenderDictionaryWithDatesSingleCurrency = expensesUserIsLender.ToDictionary
    (e => e.ExpenseTime,
      e => e.Participants.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]) ?
      new Money(e.Payers.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).PaymentAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO) -
      new Money(e.Participants.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).ParticipationAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO) : 
      new Money(e.Payers.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).PaymentAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO));

    var expensesUserIsBorrowerDictionaryWithDatesSingleCurrency = expensesUserIsBorrower.ToDictionary
    (e => e.ExpenseTime,
      e => e.Payers.Any(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]) ?
      new Money(e.Participants.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).ParticipationAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO)-
      new Money(e.Payers.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).PaymentAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO) : 
      new Money(e.Participants.FirstOrDefault(p => p.MemberId == groupIdToMemberIdMap[e.GroupId]).ParticipationAmount.ToDecimal() / exchangeRates[e.Id], requestedCurrencyISO));

    var transfersUserIsSenderDictionaryWithDatesSingleCurrency =
      transfersUserIsSender.Where(t => t.SenderId == groupIdToMemberIdMap[t.GroupId])
      .ToDictionary(
      t => t.TransferTime,
      t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], requestedCurrencyISO));

    var transfersUserIsReceiverDictionaryWithDatesSingleCurrency =
      transfersUserIsReceiver.Where(t => t.ReceiverId == groupIdToMemberIdMap[t.GroupId])
      .ToDictionary(
      t => t.TransferTime,
      t => new Money(t.Amount.ToDecimal() / exchangeRates[t.Id], requestedCurrencyISO));

    var dateRange = Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1)
      .Select(offset => startDate.AddDays(offset).Date);

    // Create a dictionary with all dates initialized to Money(0, requestedCurrencyISO)
    var defaultValues = dateRange.ToDictionary(date => date, _ => new Money(0, requestedCurrencyISO));

    var mergedDictionaryUserIsLender = defaultValues
      .Concat(expensesUserIsLenderDictionaryWithDatesSingleCurrency)
      .Concat(transfersUserIsSenderDictionaryWithDatesSingleCurrency)
      .GroupBy(kvp => kvp.Key.Date)  // Group by date without time
      .Select(groupedItem =>
    {
      var totalAmount = Money.Total(groupedItem.Select(g => g.Value));
      return new KeyValuePair<DateTime, Money>(groupedItem.Key, totalAmount);
    })
    .OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

     var mergedDictionaryUserIsBorrower = defaultValues
      .Concat(expensesUserIsBorrowerDictionaryWithDatesSingleCurrency)
      .Concat(transfersUserIsReceiverDictionaryWithDatesSingleCurrency)
      .GroupBy(kvp => kvp.Key.Date)  // Group by date without time
      .Select(groupedItem =>
    {
      var totalAmount = Money.Total(groupedItem.Select(g => g.Value));
      return new KeyValuePair<DateTime, Money>(groupedItem.Key, totalAmount);
    })
    .OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    var totalLentCumulativeArray = mergedDictionaryUserIsLender
      .Aggregate(new List<Money>(), (acc, kvp) =>
      {
        var previousTotal = acc.Count > 0 ? acc.Last() : Money.Zero(requestedCurrencyISO);
        var currentTotal = previousTotal + kvp.Value;
        acc.Add(currentTotal);
        return acc;
      });

    var totalBorrowedCumulativeArray = mergedDictionaryUserIsBorrower
    .Aggregate(new List<Money>(), (acc, kvp) =>
    {
      var previousTotal = acc.Count > 0 ? acc.Last() : Money.Zero(requestedCurrencyISO);
      var currentTotal = previousTotal + kvp.Value;
      acc.Add(currentTotal);
      return acc;
    });


    return Result.Success((totalLentCumulativeArray, totalBorrowedCumulativeArray));
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
    var allRates = allRatesMaybe;

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

        var previousMonth = currentDate.AddMonths(-1);

        startDate = (currentDate.Day - day2Int) switch
        {
          >= 0 => new DateTime(currentDate.Year, currentDate.Month, Math.Min(day2Int, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))),
          _ => currentDate.Month == 1  //check if January so it will go to Dec of prev year
              ? new DateTime(previousMonth.Year - 1, 12, Math.Min(day2Int, DateTime.DaysInMonth(previousMonth.Year - 1, 12)))// If we are in January, go back to December of the previous year
              : new DateTime(previousMonth.Year, previousMonth.Month, Math.Min(day2Int, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month)))// Go back to the previous month of the current year
        };

        var tempEndDate = startDate.AddMonths(1);
        //case where previous month has 30 and next month 31 days.
        //February does not have an issue as regardless at what day the previous month ends
        //adding a month to it will alwasy bring to the 28 or 29 of Feb.
        endDate = (tempEndDate.Day == 30 && day2Int == 31 && tempEndDate.AddDays(1).Day == 31) ? tempEndDate.AddDays(1) : tempEndDate;

        if (endDate < currentDate)
        {
          startDate = endDate.Date;
          endDate = startDate.AddMonths(1);
        };

        break;

      case BudgetType.Weekly:
        var dayDifference = (int)currentDate.DayOfWeek - day2Int;

        startDate = dayDifference switch
        {
          >= 0 => currentDate.AddDays(-dayDifference),
          _ => currentDate.AddDays(-dayDifference - 7)
        };

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