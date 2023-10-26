using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.ExchangeRateRepository;
using NMoneys;
using SplitBackApi.Api.Helper;


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
    var budgetCurrencyISO = Enum.Parse<CurrencyIsoCode>(budgetCurrency);

    var groupIdToMemberIdMap = groups.ToDictionary(
    group => group.Id,
    group => UserIdToMemberIdHelper.UserIdToMemberId(group, authenticatedUserId));

    var expenses = await _expenseRepository.GetLatestByGroupsIdsMembersIdsAndStartDate(groupIdToMemberIdMap, startDate);
    var transfers = await _transferRepository.GetLatestByGroupsIdsMembersIdsAndStartDate(groupIdToMemberIdMap, startDate);
    var exchangeRates = await GetAllRatesFromAllOperations(expenses, transfers, budgetCurrency, _exchangeRateRepository);

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
          int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
          startDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(day2Int, daysInMonth));
        }
        else
        {
          // Calculate the previous month
          var previousMonth = currentDate.AddMonths(-1);

          if (currentDate.Month == 1) //check if January so it will go to Dec of prev year
          {
            // If we are in January, go back to December of the previous year
            int daysInMonth = DateTime.DaysInMonth(previousMonth.Year - 1, 12);
            startDate = new DateTime(previousMonth.Year - 1, 12, Math.Min(day2Int, daysInMonth));
          }
          else
          {
            // Go back to the previous month of the current year
            int daysInMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
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
        int dayDifference = (int)currentDate.DayOfWeek - day2Int;

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
    double remainingDays;

    switch (budgetType)
    {
      case BudgetType.Monthly:

        remainingDays = (startDate.AddMonths(1) - currentDate).TotalDays;

        break;

      case BudgetType.Weekly:

        remainingDays = (startDate.AddDays(7) - currentDate).TotalDays;

        break;

      default:
        throw new NotImplementedException("Unsupported budget type");
    }
    return Result.Success(remainingDays);

  }









}