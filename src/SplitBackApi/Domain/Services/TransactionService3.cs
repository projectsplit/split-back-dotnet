using CSharpFunctionalExtensions;
using NMoneys;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Services;

public class TransactionService3
{
  private readonly IGroupRepository _groupRepository;
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  public TransactionService3(
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

      // New empty timeline for current currency & initialize totals
      var transactionTimelineForCurrency = new List<TransactionTimelineItem2>();
      Money totalLentSoFar = Money.Zero(isoCurrency);
      Money totalBorrowedSoFar = Money.Zero(isoCurrency);

      // Loop sortedTransactionMemberDetails created before
      sortedTransactionMemberDetails.ToList()
      .ForEach(transactionMemberDetail =>
      {
        totalLentSoFar = totalLentSoFar.Plus(transactionMemberDetail.Lent);
        totalBorrowedSoFar = totalBorrowedSoFar.Plus(transactionMemberDetail.Borrowed);

        transactionTimelineForCurrency.Add(transactionMemberDetail.ToTransactionTimelineItem2(totalLentSoFar, totalBorrowedSoFar));
      });

      transactionTimelinePerCurrency.Add(currency, transactionTimelineForCurrency);
    });

    return transactionTimelinePerCurrency;
  }

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

      var transactionMembers = new List<TransactionMember2>();
      var isoCurrency = currency.StringToIsoCode();

      group.Members.ToList().ForEach(member => transactionMembers
      .Add(new TransactionMember2(member.MemberId, Money.Zero(isoCurrency), Money.Zero(currency))));

      var currencyExpenses = expenses.Where(e => e.Currency == currency).ToList();

      currencyExpenses.ForEach(expense =>
      {
        expense.Participants.ToList().ForEach(participant =>
        {
          transactionMembers.Single(m => m.Id == participant.MemberId)
          .TotalAmountTaken += new Money(participant.ParticipationAmount.ToDecimal(), isoCurrency);
        });

        expense.Payers.ToList().ForEach(payer =>
        {
         transactionMembers.Single(m => m.Id == payer.MemberId)
         .TotalAmountGiven += new Money(payer.PaymentAmount.ToDecimal(), isoCurrency);
        });
      });

      transfers.Where(t => t.Currency == currency).ToList()
      .ForEach(transfer =>
      {
        transactionMembers.Single(m => m.Id == transfer.ReceiverId).TotalAmountTaken += new Money(transfer.Amount.ToDecimal(), isoCurrency);
        transactionMembers.Single(m => m.Id == transfer.SenderId).TotalAmountGiven += new Money(transfer.Amount.ToDecimal(), isoCurrency);
      });

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
    });
    return pendingTransactions;
  }
}