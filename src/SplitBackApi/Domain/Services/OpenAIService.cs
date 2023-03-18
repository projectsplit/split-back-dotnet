using CSharpFunctionalExtensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Services;

public class OpenAIService {

  private readonly IGroupRepository _groupRepository;
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  private readonly IUserRepository _userRepository;

  public OpenAIService(
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

  public async Task<Result<List<ExplanationText>>> GenerateChatScriptAsync(string groupId) { //GenerateChatScript

    var groupResult = await _groupRepository.GetById(groupId);
    if(groupResult.IsFailure) return Result.Failure<List<ExplanationText>>(groupResult.Error);

    var group = groupResult.Value;
    //var memberIds = group.Members.Select(m => m.MemberId).ToList();
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct();

    var pendingTransactions = new List<PendingTransaction>();
    var explanationTexts = new List<ExplanationText>();

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

      //var retainedDebtors = new Queue<TransactionMember>(debtors.ToList());
      //var retainedCreditors = new Queue<TransactionMember>(creditors.ToList());

      var retainedDebtors = new Queue<TransactionMemberWrapper>(debtors.Select(member => new TransactionMemberWrapper(member, member.TotalAmountTaken - member.TotalAmountGiven)).ToList());
      var retainedCreditors = new Queue<TransactionMemberWrapper>(creditors.Select(member => new TransactionMemberWrapper(member, member.TotalAmountGiven - member.TotalAmountTaken)).ToList());

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

      //only use Ids in pendingTransactions.
      List<string> senderIds = pendingTransactions.Select(t => t.SenderId).Distinct().ToList();
      List<string> receiverIds = pendingTransactions.Select(t => t.ReceiverId).Distinct().ToList();
      List<string> memberIds = senderIds.Union(receiverIds).ToList();

      foreach(var memberId in memberIds) {

        var memberIsCreditor = retainedCreditors.Any(c => c.Member.Id == memberId);

        if(memberIsCreditor) {

          var memberInfo = retainedCreditors.FirstOrDefault(c => c.Member.Id == memberId);
          var member = memberInfo.Member;

          var memberNameResult = await IdToName.MemberIdToMemberName(groupId, memberId, _groupRepository, _userRepository);
          if(memberNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(memberNameResult.Error);
          var memberName = memberNameResult.Value;

          explanationTexts.Add(new ExplanationText {
            Txt = $" The total amount {memberName} paid for the group is {member.TotalAmountGiven} {currency}.{(member.TotalAmountTaken == 0 ? $"{memberName} received nothing from the group in {currency}." : $"The total amount {memberName} received from the group is {member.TotalAmountTaken} {currency}.")}" +
                   $"As a result, {memberName} is a creditor with a credit of {member.TotalAmountGiven - member.TotalAmountTaken} {currency}."
          });
        }

        var memberIsDebtor = retainedDebtors.Any(d => d.Member.Id == memberId);

        if(memberIsDebtor) {

          var memberInfo = retainedDebtors.FirstOrDefault(d => d.Member.Id == memberId);
          var member = memberInfo.Member;

          var memberNameResult = await IdToName.MemberIdToMemberName(groupId, memberId, _groupRepository, _userRepository);
          if(memberNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(memberNameResult.Error);
          var memberName = memberNameResult.Value;

          explanationTexts.Add(new ExplanationText {
            Txt = $"{(member.TotalAmountGiven == 0 ? $"{memberName} paid nothing for the group in {currency}." : $"The total amount {memberName} paid for the group is {member.TotalAmountGiven}{currency}.")} The total amount {memberName} received from the group is {member.TotalAmountTaken} {currency}." +
                  $"As a result {memberName} is a debtor with a debt of {member.TotalAmountTaken - member.TotalAmountGiven} {currency}."
          });
        }

        if(!memberIsCreditor && !memberIsDebtor) {
          explanationTexts.Add(new ExplanationText {
            Txt = ""
          });
        }
      }

      foreach(var p in pendingTransactions.Where(pt => pt.Currency == currency)) {

        var debtorNameResult = await IdToName.MemberIdToMemberName(groupId, p.SenderId, _groupRepository, _userRepository);
        if(debtorNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(debtorNameResult.Error);
        var debtorName = debtorNameResult.Value;

        var creditorNameResult = await IdToName.MemberIdToMemberName(groupId, p.ReceiverId, _groupRepository, _userRepository);
        if(creditorNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(creditorNameResult.Error);
        var creditorName = creditorNameResult.Value;

        var debtor = retainedDebtors.FirstOrDefault(d => d.Member.Id == p.SenderId);
        var creditor = retainedCreditors.FirstOrDefault(d => d.Member.Id == p.ReceiverId);

        if(debtor.Remainder == p.Amount) {
          if(creditor.Remainder == p.Amount) {

            explanationTexts.Add(new ExplanationText {
              Txt = $"{debtorName} owes {p.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount} {currency} to {creditorName} " +
                    $"{debtorName}'s debt and {creditorName}'s credit will be cleared."
            });
          }
          if(creditor.Remainder > p.Amount) {

            explanationTexts.Add(new ExplanationText {
              Txt = $"{debtorName} owes {p.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount} {currency} to {creditorName} " +
                    $"{debtorName}'s debt will be cleared and {creditorName}'s credit will be reduced to {creditor.Remainder - p.Amount} {currency}."
            });

            creditor.Remainder = creditor.Remainder - p.Amount;
          }
        }
        if(debtor.Remainder > p.Amount) {
          if(creditor.Remainder == p.Amount) {

            explanationTexts.Add(new ExplanationText {
              Txt = $"{debtorName} owes {p.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount} {currency} to {creditorName} " +
                    $"it will reduce {debtorName}'s debt to {debtor.Remainder - p.Amount} {currency} and {creditorName}'s credit will be cleared."
            });

            debtor.Remainder = debtor.Remainder - p.Amount;
          }

          if(creditor.Remainder > p.Amount) {

            explanationTexts.Add(new ExplanationText {
              Txt = $"{debtorName} owes {p.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount} {currency} to {creditorName} " +
                    $"it will reduce {debtorName}'s debt to {debtor.Remainder - p.Amount} {currency} and {creditorName}'s credit will be reduced to {creditor.Remainder - p.Amount}."
            });

            creditor.Remainder = creditor.Remainder - p.Amount;
            debtor.Remainder = debtor.Remainder - p.Amount;
          }
        }
      }
    }
    return explanationTexts;
  }



  public async Task<Result<List<ExplanationText>>> GenerateTransactionsExplanationTextAsync(string groupId) {

    var groupResult = await _groupRepository.GetById(groupId);
    if(groupResult.IsFailure) return Result.Failure<List<ExplanationText>>(groupResult.Error);

    var group = groupResult.Value;
    var memberIds = group.Members.Select(m => m.MemberId).ToList();
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct();

    var pendingTransactions = new List<PendingTransaction>();
    var explanationTexts = new List<ExplanationText>();

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

      var retainedDebtors = new Queue<TransactionMember>(debtors.ToList()); ;
      var retainedCreditors = new Queue<TransactionMember>(creditors.ToList());

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

      //only use Ids in pendingTransactions.
      foreach(var memberId in memberIds) {

        var memberIsCreditor = retainedCreditors.Any(c => c.Id == memberId);

        if(memberIsCreditor) {

          var memberInfo = retainedCreditors.FirstOrDefault(c => c.Id == memberId);

          var pendingTransactionsMemberAsCreditor = pendingTransactions.Where(pt => pt.ReceiverId == memberId && pt.Currency == currency).ToList();

          var memberNameResult = await IdToName.MemberIdToMemberName(groupId, memberId, _groupRepository, _userRepository);
          if(memberNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(memberNameResult.Error);
          var memberName = memberNameResult.Value;

          explanationTexts.Add(new ExplanationText {
            Txt = $" The total amount {memberName} paid for the group is {memberInfo.TotalAmountGiven} {currency}.{(memberInfo.TotalAmountTaken == 0 ? $"{memberName} received nothing from the group in {currency}." : $"The total amount {memberName} received from the group is {memberInfo.TotalAmountTaken} {currency}.")}" +
                   $"As a result, {memberName} is a creditor with a credit of {memberInfo.TotalAmountGiven - memberInfo.TotalAmountTaken} {currency}."
          });

          var memberCredit = memberInfo.TotalAmountGiven - memberInfo.TotalAmountTaken;

          foreach(var p in pendingTransactionsMemberAsCreditor) {

            var debtor = retainedDebtors.FirstOrDefault(d => d.Id == p.SenderId);
            var debtorAmount = debtor.TotalAmountTaken - debtor.TotalAmountGiven;

            var senderNameResult = await IdToName.MemberIdToMemberName(groupId, p.SenderId, _groupRepository, _userRepository);
            if(senderNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(senderNameResult.Error);
            var senderName = senderNameResult.Value;

            if(debtor.Id == p.SenderId) {
              if(debtor.TotalAmountTaken - debtor.TotalAmountGiven == p.Amount) { //debt==amount
                memberCredit = memberCredit - debtorAmount;
                explanationTexts.Add(new ExplanationText {
                  Txt =
                   $"{(debtor.TotalAmountGiven == 0 ? $"{senderName} paid nothing for the group in {currency}." : $"{senderName}'s total amount paid for the group is {debtor.TotalAmountGiven} {currency}.")}" +
                   $" The total amount {senderName} received from the group is {debtor.TotalAmountTaken}, therefore {senderName} is a debtor, with debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency}. " +
                   //$"This debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency} is paid from {senderName} to {memberName}, reducing {memberName}'s credit to {memberCredit} {currency}. "
                   //$"{senderName} pays this debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency} to {memberName}, reducing {memberName}'s credit to {memberCredit} {currency}."
                   $"If {senderName} pays this debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency} to {memberName}, {(memberCredit == 0 ? $"{memberName}'s credit will be cleared." : $"{memberName}'s credit will be reduced to {memberCredit} {currency}.")}"
                  //+ $"{memberName}'s credit will be reduced to {memberCredit} {currency}."
                });
              } else if(debtor.TotalAmountTaken - debtor.TotalAmountGiven > p.Amount) {

                explanationTexts.Add(new ExplanationText {
                  Txt =
                   $"{(debtor.TotalAmountGiven == 0 ? $"{senderName} paid nothing for the group in {currency}." : $"{senderName}'s total amount paid for the group is {debtor.TotalAmountGiven} {currency}.")}" +
                   $" The total amount {senderName} received from the group is {debtor.TotalAmountTaken} {currency}, therefore {senderName} is a debtor, with debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency}. " +
                   $"If {memberName} takes {p.Amount} from {senderName}, {memberName}'s credit will be cleared."//.{senderName} reduces their debt to {debtor.TotalAmountTaken - debtor.TotalAmountGiven - p.Amount}{currency} as a result. "
                });
              }
            }
          }
        }

        var memberIsDebtor = retainedDebtors.Any(d => d.Id == memberId);
        if(memberIsDebtor) {

          var memberInfo = retainedDebtors.FirstOrDefault(d => d.Id == memberId);

          var pendingTransactionsMemberAsDebtor = pendingTransactions.Where(pt => pt.SenderId == memberId && pt.Currency == currency).ToList();

          var memberNameResult = await IdToName.MemberIdToMemberName(groupId, memberId, _groupRepository, _userRepository);
          if(memberNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(memberNameResult.Error);
          var memberName = memberNameResult.Value;

          explanationTexts.Add(new ExplanationText {
            Txt = $"{(memberInfo.TotalAmountGiven == 0 ? $"{memberName} paid nothing for the group in {currency}." : $"The total amount {memberName} paid for the group is {memberInfo.TotalAmountGiven}{currency}.")} The total amount {memberName} received from the group is {memberInfo.TotalAmountTaken} {currency}." +
                  $"As a result {memberName} is a debtor with a debt of {memberInfo.TotalAmountTaken - memberInfo.TotalAmountGiven} {currency}."
          });
          var memberDebt = memberInfo.TotalAmountTaken - memberInfo.TotalAmountGiven;

          foreach(var p in pendingTransactionsMemberAsDebtor) {

            var creditor = retainedCreditors.FirstOrDefault(c => c.Id == p.ReceiverId);
            var creditorAmount = creditor.TotalAmountGiven - creditor.TotalAmountTaken;

            var receiverNameResult = await IdToName.MemberIdToMemberName(groupId, p.ReceiverId, _groupRepository, _userRepository);
            if(receiverNameResult.IsFailure) return Result.Failure<List<ExplanationText>>(receiverNameResult.Error);
            var receiverName = receiverNameResult.Value;

            if(creditor.Id == p.ReceiverId) {
              if(creditor.TotalAmountGiven - creditor.TotalAmountTaken == p.Amount) { //debt==amount
                memberDebt = memberDebt - creditorAmount;
                //need in next loop this to be membersReducedDebt = membersReducedDebt - (creditor.TotalAmountGive - creditor.TotalAmountTaken)
                explanationTexts.Add(new ExplanationText {
                  Txt =
                   $"{receiverName}'s total amount paid for the group is {creditor.TotalAmountGiven} {currency}. " +
                   $"{(creditor.TotalAmountTaken == 0 ? $"{receiverName} did not receive anything from the group in {currency}." : $"The total amount {receiverName} received from the group is {creditor.TotalAmountTaken} {currency}.")}" +
                   $"As a result {receiverName} is a creditor, with a credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency}. " +
                   //$"This credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency} is paid from {memberName} to {receiverName}, reducing {memberName}'s debt to {memberDebt} {currency}."
                   //$"{memberName} pays this credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency} to {receiverName}, reducing {memberName}'s debt to {memberDebt} {currency}."
                   $"If {memberName} pays this credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency} to {receiverName}, {(memberDebt == 0 ? $"{memberName}'s debt will be cleared." : $"{memberName}'s debt will be reduced to {memberDebt} {currency}.")}"
                });
              } else if(creditor.TotalAmountGiven - creditor.TotalAmountTaken > p.Amount) {

                explanationTexts.Add(new ExplanationText {
                  Txt =
                   $"{receiverName}'s total amount paid for the group is {creditor.TotalAmountGiven} {currency}. " +
                   $"{(creditor.TotalAmountTaken == 0 ? $"{receiverName} did not receive anything from the group in {currency}." : $"The total amount {receiverName} received from the group is {creditor.TotalAmountTaken} {currency}.")}" +
                   $"As a result {receiverName} is a creditor, with a credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency}. " +
                   $"If {memberName} pays {p.Amount} {currency} to {receiverName}, {memberName}'s debt will be cleared." //and reducing {receiverName}'s credit to {creditor.TotalAmountGiven - creditor.TotalAmountTaken - p.Amount}{currency}."
                });
              }
            }
          }
        }

        if(!memberIsCreditor && !memberIsDebtor) {
          explanationTexts.Add(new ExplanationText {
            Txt = ""
          });
        }
      }
    }

    return explanationTexts;
  }

}