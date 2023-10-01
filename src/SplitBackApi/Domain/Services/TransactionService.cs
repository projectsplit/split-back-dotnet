using CSharpFunctionalExtensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using NMoneys;

namespace SplitBackApi.Domain.Services;

public class TransactionService {

  private readonly IGroupRepository _groupRepository;
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  private readonly IUserRepository _userRepository;

  public TransactionService(
    IGroupRepository groupRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IUserRepository userRepository
  ) {
    _groupRepository = groupRepository;
    _expenseRepository = expenseRepository;
    _transferRepository = transferRepository;
    _userRepository = userRepository;
  }

  public async Task<Result<Dictionary<string, List<TransactionTimelineItem>>>> GetTransactionHistory(string groupId, string memberId) {

    var groupMaybe = await _groupRepository.GetById(groupId);
    if(groupMaybe.HasNoValue) return Result.Failure<Dictionary<string, List<TransactionTimelineItem>>>("Group not found");

    var group = groupMaybe.Value;
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct();

    var transactionTimelinePerCurrency = new Dictionary<string, List<TransactionTimelineItem>>();

    // Loop currencies used
    foreach(var currency in allCurrencies) {
      // New list of TransactionMemberDetail
      var transactionMemberDetails = new List<TransactionMemberDetail>();

      // Loop all expenses & add to list
      foreach(var expense in expenses.Where(e => e.Currency == currency)) {
        var transactionMemberDetail = expense.ToTransactionMemberDetailFromUserId(memberId);

        if(transactionMemberDetail is not null) {
          transactionMemberDetails.Add(transactionMemberDetail);
        }
      };

      // Loop all transfers & add to list
      foreach(var transfer in transfers.Where(t => t.Currency == currency)) {
        var transactionMemberDetail = transfer.ToTransactionMemberDetailFromUserId(memberId);

        if(transactionMemberDetail is not null) {
          transactionMemberDetails.Add(transactionMemberDetail);
        }
      };

      // Sort list
      var sortedTransactionMemberDetails = transactionMemberDetails.OrderBy(t => t.TransactionTime);

      // New empty timeline for current currency & initialize totals
      var transactionTimelineForCurrency = new List<TransactionTimelineItem>();
      var totalLentSoFar = 0m;
      var totalBorrowedSoFar = 0m;

      // Loop sortedTransactionMemberDetails created before
      foreach(var transactionMemberDetail in sortedTransactionMemberDetails) {

        totalLentSoFar += transactionMemberDetail.Lent;
        totalBorrowedSoFar += transactionMemberDetail.Borrowed;

        transactionTimelineForCurrency.Add(transactionMemberDetail.ToTransactionTimelineItem(totalLentSoFar, totalBorrowedSoFar));
      };

      transactionTimelinePerCurrency.Add(currency, transactionTimelineForCurrency);
    }

    return transactionTimelinePerCurrency;
  }

  public async Task<Result<List<PendingTransaction>>> PendingTransactionsAsync(string groupId) {

    var groupMaybe = await _groupRepository.GetById(groupId);
    if(groupMaybe.HasNoValue) return Result.Failure<List<PendingTransaction>>("Group not found");

    var group = groupMaybe.Value;
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct();

    var pendingTransactions = new List<PendingTransaction>();

    foreach(var currency in allCurrencies) {

      var transactionMembers = new List<TransactionMember>();

      foreach(var member in group.Members) {
        transactionMembers.Add(new TransactionMember(member.MemberId, 0m, 0m));
      }

      foreach(var expense in expenses.Where(e => e.Currency == currency)) {

        foreach(var participant in expense.Participants) {
          transactionMembers.Single(m => m.Id == participant.MemberId).TotalAmountTaken += participant.ParticipationAmount.ToDecimal();
        }

        foreach(var payer in expense.Payers) {
          transactionMembers.Single(p => p.Id == payer.MemberId).TotalAmountGiven += payer.PaymentAmount.ToDecimal();
        }
      }

      foreach(var transfer in transfers.Where(t => t.Currency == currency)) {
        transactionMembers.Single(m => m.Id == transfer.ReceiverId).TotalAmountTaken += transfer.Amount.ToDecimal();
        transactionMembers.Single(m => m.Id == transfer.SenderId).TotalAmountGiven += transfer.Amount.ToDecimal();
      }

      var debtors = new Queue<TransactionMember>();
      var creditors = new Queue<TransactionMember>();

      foreach(var transactionMember in transactionMembers) {

        switch(transactionMember.TotalAmountGiven - transactionMember.TotalAmountTaken) {
          case < 0:
            debtors.Enqueue(transactionMember);
            break;

          case > 0:
            creditors.Enqueue(transactionMember);
            break;
        }
      }

      while(debtors.Count > 0 && creditors.Count > 0) {

        var poppedDebtor = debtors.Dequeue();
        var poppedCreditor = creditors.Dequeue();

        var debt = (poppedDebtor.TotalAmountTaken - poppedDebtor.TotalAmountGiven);
        var credit = (poppedCreditor.TotalAmountGiven - poppedCreditor.TotalAmountTaken);
        var diff = debt - credit;

        switch(diff) {

          case < 0:
            pendingTransactions.Add(new PendingTransaction {
              SenderId = poppedDebtor.Id,
              ReceiverId = poppedCreditor.Id,
              Amount = debt,
              Currency = currency
            });

            creditors.Enqueue(poppedCreditor with { TotalAmountTaken = poppedCreditor.TotalAmountTaken + debt });
            break;

          case > 0:
            pendingTransactions.Add(new PendingTransaction {
              SenderId = poppedDebtor.Id,
              ReceiverId = poppedCreditor.Id,
              Amount = credit,
              Currency = currency
            });

            debtors.Enqueue(poppedDebtor with { TotalAmountGiven = poppedDebtor.TotalAmountGiven + credit });
            break;

          case 0:
            pendingTransactions.Add(new PendingTransaction {
              SenderId = poppedDebtor.Id,
              ReceiverId = poppedCreditor.Id,
              Amount = credit, //credit == debt
              Currency = currency
            });
            break;
        }
      }
    }
    return pendingTransactions;
  }
}