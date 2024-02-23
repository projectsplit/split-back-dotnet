using CSharpFunctionalExtensions;
using NMoneys;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Services;

public class TransactionService2
{
  private readonly IGroupRepository _groupRepository;
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  public TransactionService2(
    IGroupRepository groupRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository
  )
  {
    _groupRepository = groupRepository;
    _expenseRepository = expenseRepository;
    _transferRepository = transferRepository;
  }

  public async Task<Result<Dictionary<string, List<TransactionTimelineItem2>>>> GetTransactionHistory(string groupId, string memberId)
  {

    var groupResult = await _groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Result.Failure<Dictionary<string, List<TransactionTimelineItem2>>>(groupResult.Error);

    var group = groupResult.Value;
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct().ToList();

    var transactionTimelinePerCurrency = new Dictionary<string, List<TransactionTimelineItem2>>();

    //Loop curencies used
    allCurrencies.ForEach(currency =>
    {
      // New list of TransactionMemberDetail
      var transactionMemberDetails = new List<TransactionMemberDetail2>();
      var isoCurrency = currency.StringToIsoCode();

      // Loop all expenses & add to list
      expenses.Where(e => e.Currency == currency).ToList().ForEach(expense =>
      {
        var transactionMemberDetail = expense.ToTransactionMemberDetailFromUserId2(memberId);
        transactionMemberDetails?.Add(transactionMemberDetail);

      });
      // Loop all transfers & add to list
      transfers.Where(t => t.Currency == currency).ToList().ForEach(transfer =>
      {
        var transactionMemberDetail = transfer.ToTransactionMemberDetailFromUserId2(memberId);
        transactionMemberDetails?.Add(transactionMemberDetail);
      });

      // Sort list
      var sortedTransactionMemberDetails = transactionMemberDetails.OrderBy(t => t.TransactionTime);

      var transactionTimelineForCurrency = sortedTransactionMemberDetails
      .Aggregate(
        new List<TransactionTimelineItem2>(),
        (transactionTimelineItems, transactionMemberDetail) =>
        {
          var totalLent = new Money(transactionTimelineItems.Sum(item => item.TotalLent.Amount), currency).Plus(transactionMemberDetail.Lent);
          var totalBorrowed = new Money(transactionTimelineItems.Sum(item => item.TotalBorrowed.Amount), currency).Plus(transactionMemberDetail.Borrowed);

          var transactionTimelineItem = transactionMemberDetail.ToTransactionTimelineItem2(totalLent, totalBorrowed);

          return transactionTimelineItems.Concat(new[] { transactionTimelineItem }).ToList();
        });

      transactionTimelinePerCurrency.Add(currency, transactionTimelineForCurrency);
    });

    return transactionTimelinePerCurrency;
  }
  // var totalLentSoFar2 = Money.Total(sortedTransactionMemberDetails.Select(t => t.Lent));
  // var totalBorrowedSoFar2 =  Money.Total(sortedTransactionMemberDetails.Select(t => t.Borrowed));
  public async Task<Result<List<PendingTransaction2>>> PendingTransactionsAsync2(string groupId)
  {
    var groupResult = await _groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Result.Failure<List<PendingTransaction2>>(groupResult.Error);

    var group = groupResult.Value;
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct().ToList();

    var pendingTransactions = new List<PendingTransaction2>();

    allCurrencies.ForEach(currency =>
    {
      var groupMembers = group.Members.ToList();

      var transactionMembers = CalculateTotalTakenAndGivenForEachTransactionMember(groupMembers, expenses, transfers, currency);

      var debtors = new Queue<TransactionMember2>();
      var creditors = new Queue<TransactionMember2>();

      (debtors, creditors) = CalculateDebtorsAndCreditors(transactionMembers);

      CalculatePendingTransactions(debtors, creditors, pendingTransactions, currency);

    });

    return pendingTransactions;
  }

  public async Task<Result<List<PendingTransaction2>>> PendingTransactionsForAllGroupsAsync(List<Group> groups)
  {
    var allExpenses = await _expenseRepository.GetByGroupIds(groups.Select(g => g.Id).ToList());
    var allTransfers = await _transferRepository.GetByGroupIds(groups.Select(g => g.Id).ToList());

    var allCurrencies =
      allExpenses.Select(e => e.Currency)
      .Concat(allTransfers.Select(t => t.Currency))
      .Distinct().ToList();

    var pendingTransactions = new List<PendingTransaction2>();

    allCurrencies.ForEach(currency =>
    {
      var groupMembers = groups.SelectMany(g => g.Members).ToList();

      var transactionMembers = CalculateTotalTakenAndGivenForEachTransactionMember(groupMembers, allExpenses, allTransfers, currency);

      var debtors = new Queue<TransactionMember2>();
      var creditors = new Queue<TransactionMember2>();

      (debtors, creditors) = CalculateDebtorsAndCreditors(transactionMembers);

      CalculatePendingTransactions(debtors, creditors, pendingTransactions, currency);

    });

    return pendingTransactions;
  }

  public static List<TransactionMember2> CalculateTotalTakenAndGivenForEachTransactionMember(List<Member> members, List<Expense> expenses, List<Transfer> transfers, string currency)
  {
    var isoCurrency = currency.StringToIsoCode();
    var transactionMembers = new List<TransactionMember2>();

    members.ForEach(m =>
    {
      var currencyExpenses = expenses.Where(expense => expense.Currency == currency);
      var currencyTransfersSender = transfers.Where(transfer => transfer.Currency == currency && transfer.SenderId == m.MemberId);
      var currencyTransfersReceiver = transfers.Where(transfer => transfer.Currency == currency && transfer.ReceiverId == m.MemberId);

      var TotalParticipationAmountFromExpenses = currencyExpenses.Any() ?
          currencyExpenses
              .SelectMany(expense => expense.Participants).Where(p => p.MemberId == m.MemberId)
              .Select(p => p.ParticipationAmount)
              .Select(amount => new Money(amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

      var TotalAmountGivenFromTransfers = currencyTransfersSender.Any() ?
          currencyTransfersSender
              .Select(t => new Money(t.Amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

      var TotalPaymentAmountFromExpenses = currencyExpenses.Any() ?
          currencyExpenses
              .SelectMany(expense => expense.Payers).Where(p => p.MemberId == m.MemberId)
              .Select(p => p.PaymentAmount)
              .Select(amount => new Money(amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

      var TotalAmountTakenFromTransfers = currencyTransfersReceiver.Any() ?
          currencyTransfersReceiver
              .Select(t => new Money(t.Amount.ToDecimal(), isoCurrency)) : new[] { Money.Zero(isoCurrency) };

      transactionMembers.Add(new TransactionMember2(
          m.MemberId,
          Money.Total(TotalPaymentAmountFromExpenses.Concat(TotalAmountGivenFromTransfers)),
          Money.Total(TotalParticipationAmountFromExpenses.Concat(TotalAmountTakenFromTransfers))
      ));
    });

    return transactionMembers;
  }

  public static (Queue<TransactionMember2>, Queue<TransactionMember2>) CalculateDebtorsAndCreditors(List<TransactionMember2> transactionMembers)
  {
    var debtors = new Queue<TransactionMember2>();
    var creditors = new Queue<TransactionMember2>();

    transactionMembers.ForEach(transactionMember =>
       {
         switch (transactionMember.TotalAmountGiven.Minus(transactionMember.TotalAmountTaken).Amount)
         {
           case < 0:
             debtors.Enqueue(transactionMember);
             break;

           case > 0:
             creditors.Enqueue(transactionMember);
             break;
         }
       });
    return (debtors, creditors);
  }

  public static void CalculatePendingTransactions(
    Queue<TransactionMember2> debtors,
    Queue<TransactionMember2> creditors,
    List<PendingTransaction2> pendingTransactions,
    string currency)
  {
    while (debtors.Count > 0 && creditors.Count > 0)
    {

      var poppedDebtor = debtors.Dequeue();
      var poppedCreditor = creditors.Dequeue();

      var debt = poppedDebtor.TotalAmountTaken.Minus(poppedDebtor.TotalAmountGiven);
      var credit = poppedCreditor.TotalAmountGiven.Minus(poppedCreditor.TotalAmountTaken);
      var diff = debt.Minus(credit).Amount;

      switch (diff)
      {
        case < 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = debt,
            Currency = currency
          });

          creditors.Enqueue(poppedCreditor with { TotalAmountTaken = poppedCreditor.TotalAmountTaken.Plus(debt) });
          break;

        case > 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = credit,
            Currency = currency
          });

          debtors.Enqueue(poppedDebtor with { TotalAmountGiven = poppedDebtor.TotalAmountGiven.Plus(credit) });
          break;

        case 0:
          pendingTransactions.Add(new PendingTransaction2
          {
            SenderId = poppedDebtor.Id,
            ReceiverId = poppedCreditor.Id,
            Amount = credit, //credit == debt
            Currency = currency
          });
          break;
      }
    }
  }
}