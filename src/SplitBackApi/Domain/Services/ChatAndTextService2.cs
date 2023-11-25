using CSharpFunctionalExtensions;
using NMoneys;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Services;

public class ChatAndTextService2
{
  private readonly IGroupRepository _groupRepository;
  private readonly IExpenseRepository _expenseRepository;
  private readonly ITransferRepository _transferRepository;
  private readonly IUserRepository _userRepository;


  public ChatAndTextService2(
    IGroupRepository groupRepository,
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IUserRepository userRepository

  )
  {
    _groupRepository = groupRepository;
    _expenseRepository = expenseRepository;
    _transferRepository = transferRepository;
    _userRepository = userRepository;

  }

  public async Task<Result<List<ExplanationText>>> GenerateChatScriptAsync(string groupId)
  { //GenerateChatScript

    var groupResult = await _groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Result.Failure<List<ExplanationText>>(groupResult.Error);

    var group = groupResult.Value;
    //var memberIds = group.Members.Select(m => m.MemberId).ToList();
    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, _userRepository);
    if (membersWithNamesResult.IsFailure) return Result.Failure<List<ExplanationText>>(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct().ToList();

    var pendingTransactions = new List<PendingTransaction2>();
    var explanationTexts = new List<ExplanationText>();

    allCurrencies.ForEach(currency =>
    {
      var groupMembers = group.Members.ToList();
      var isoCurrency = currency.StringToIsoCode();

      var transactionMembers = TransactionService2.CalculateTotalTakenAndGivenForEachTransactionMember(groupMembers, expenses, transfers, currency);

      var debtors = new Queue<TransactionMember2>();
      var creditors = new Queue<TransactionMember2>();

      (debtors, creditors) = TransactionService2.CalculateDebtorsAndCreditors(transactionMembers);

      var retainedDebtors = new Queue<TransactionMemberWrapper2>(debtors.Select(member => new TransactionMemberWrapper2(member, member.TotalAmountTaken - member.TotalAmountGiven)).ToList());
      var retainedCreditors = new Queue<TransactionMemberWrapper2>(creditors.Select(member => new TransactionMemberWrapper2(member, member.TotalAmountGiven - member.TotalAmountTaken)).ToList());

      TransactionService2.CalculatePendingTransactions(debtors, creditors, pendingTransactions, currency);

      //only use Ids in pendingTransactions.
      var senderIds = pendingTransactions.Select(t => t.SenderId).Distinct().ToList();
      var receiverIds = pendingTransactions.Select(t => t.ReceiverId).Distinct().ToList();
      var memberIds = senderIds.Union(receiverIds).ToList();

      memberIds.ForEach(mId =>
      {
        var memberIsCreditor = retainedCreditors.Any(c => c.Member.Id == mId);
        var memberIsDebtor = retainedDebtors.Any(d => d.Member.Id == mId);

        switch (memberIsCreditor, memberIsDebtor)
        {
          case (true, false):

            var memberInfo = retainedCreditors.FirstOrDefault(c => c.Member.Id == mId);
            var member = memberInfo.Member;
            var memberName = membersWithNames.Single(m => m.Id == mId).Name;

            explanationTexts.Add(new ExplanationText
            {
              Txt = $" The total amount {memberName} paid for the group is {member.TotalAmountGiven.Amount} {currency}.{(member.TotalAmountTaken == Money.Zero(isoCurrency) ? $"{memberName} received nothing from the group in {currency}." : $"The total amount {memberName} received from the group is {member.TotalAmountTaken.Amount} {currency}.")}" +
                    $"As a result, {memberName} is a creditor with a credit of {(member.TotalAmountGiven - member.TotalAmountTaken).Amount} {currency}."
            });
            break;

          case (false, true):

            memberInfo = retainedDebtors.FirstOrDefault(d => d.Member.Id == mId);
            member = memberInfo.Member;
            memberName = membersWithNames.Single(m => m.Id == mId).Name;

            explanationTexts.Add(new ExplanationText
            {
              Txt = $"{(member.TotalAmountGiven == Money.Zero(isoCurrency) ? $"{memberName} paid nothing for the group in {currency}." : $"The total amount {memberName} paid for the group is {member.TotalAmountGiven.Amount}{currency}.")} The total amount {memberName} received from the group is {member.TotalAmountTaken.Amount} {currency}." +
                    $"As a result {memberName} is a debtor with a debt of {(member.TotalAmountTaken - member.TotalAmountGiven).Amount} {currency}."
            });
            break;

          case (false, false):
            explanationTexts.Add(new ExplanationText
            {
              Txt = ""
            });
            break;
        }
      });

      var pendingTransactionsForCurrency = pendingTransactions.Where(pt => pt.Currency == currency).ToList();

      pendingTransactionsForCurrency.ForEach(p =>
      {
        var debtorName = membersWithNames.Single(m => m.Id == p.SenderId).Name;
        var creditorName = membersWithNames.Single(m => m.Id == p.ReceiverId).Name;

        var debtor = retainedDebtors.FirstOrDefault(d => d.Member.Id == p.SenderId);
        var creditor = retainedCreditors.FirstOrDefault(d => d.Member.Id == p.ReceiverId);

        switch ((debtor.Remainder - p.Amount).Amount)
        {
          case 0:

            var Txt = (creditor.Remainder - p.Amount).Amount switch
            {
              0 => $"{debtorName} owes {p.Amount.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount.Amount} {currency} to {creditorName} " +
                   $"{debtorName}'s debt and {creditorName}'s credit will be cleared.",

              > 0 => $"{debtorName} owes {p.Amount.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount.Amount} {currency} to {creditorName} " +
                     $"{debtorName}'s debt will be cleared and {creditorName}'s credit will be reduced to {(creditor.Remainder - p.Amount).Amount} {currency}.",

              _ => ""
            };

            explanationTexts.Add(new ExplanationText { Txt = Txt });
            break;

          case > 0:

            Txt = (creditor.Remainder - p.Amount).Amount switch
            {
              0 => $"{debtorName} owes {p.Amount.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount.Amount} {currency} to {creditorName} " +
                   $"it will reduce {debtorName}'s debt to {(debtor.Remainder - p.Amount).Amount} {currency} and {creditorName}'s credit will be cleared.",

              > 0 => $"{debtorName} owes {p.Amount.Amount} {currency} to {creditorName} because if {debtorName} pays {p.Amount.Amount} {currency} to {creditorName} " +
                     $"it will reduce {debtorName}'s debt to {(debtor.Remainder - p.Amount).Amount} {currency} and {creditorName}'s credit will be reduced to {(creditor.Remainder - p.Amount).Amount}.",

              _ => ""
            };

            explanationTexts.Add(new ExplanationText { Txt = Txt });
            break;
        }
      });
    });

    return explanationTexts;
  }

  public async Task<Result<List<ExplanationText>>> GenerateTransactionsExplanationTextAsync(string groupId)
  {
    var groupResult = await _groupRepository.GetById(groupId);
    if (groupResult.IsFailure) return Result.Failure<List<ExplanationText>>(groupResult.Error);

    var group = groupResult.Value;
    var memberIds = group.Members.Select(m => m.MemberId).ToList();

    var expenses = await _expenseRepository.GetByGroupId(groupId);
    var transfers = await _transferRepository.GetByGroupId(groupId);

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, _userRepository);
    if (membersWithNamesResult.IsFailure) return Result.Failure<List<ExplanationText>>(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var allCurrencies =
      expenses.Select(e => e.Currency)
      .Concat(transfers.Select(t => t.Currency))
      .Distinct().ToList();

    var pendingTransactions = new List<PendingTransaction2>();
    var explanationTexts = new List<ExplanationText>();

    allCurrencies.ForEach(currency =>
    {
      var groupMembers = group.Members.ToList();
      var isoCurrency = currency.StringToIsoCode();

     var transactionMembers= TransactionService2.CalculateTotalTakenAndGivenForEachTransactionMember(groupMembers, expenses, transfers, currency);

      var debtors = new Queue<TransactionMember2>();
      var creditors = new Queue<TransactionMember2>();

      (debtors, creditors) = TransactionService2.CalculateDebtorsAndCreditors(transactionMembers);

      var retainedDebtors = new Queue<TransactionMember2>(debtors.ToList()); ;
      var retainedCreditors = new Queue<TransactionMember2>(creditors.ToList());

      TransactionService2.CalculatePendingTransactions(debtors, creditors, pendingTransactions, currency);

      //only use Ids in pendingTransactions.
      memberIds.ForEach(memberId =>
      {
        var memberIsCreditor = retainedCreditors.Any(c => c.Id == memberId);
        var memberIsDebtor = retainedDebtors.Any(d => d.Id == memberId);

        switch (memberIsCreditor, memberIsDebtor)
        {
          case (true, false):

            var memberInfo = retainedCreditors.FirstOrDefault(c => c.Id == memberId);

            var pendingTransactionsMemberAsCreditor = pendingTransactions.Where(pt => pt.ReceiverId == memberId && pt.Currency == currency).ToList();

            var memberName = membersWithNames.Single(m => m.Id == memberId).Name;

            explanationTexts.Add(new ExplanationText
            {
              Txt = $" The total amount {memberName} paid for the group is {memberInfo.TotalAmountGiven.Amount} {currency}.{(memberInfo.TotalAmountTaken == Money.Zero(isoCurrency) ? $"{memberName} received nothing from the group in {currency}." : $"The total amount {memberName} received from the group is {memberInfo.TotalAmountTaken.Amount} {currency}.")}" +
                    $"As a result, {memberName} is a creditor with a credit of {(memberInfo.TotalAmountGiven - memberInfo.TotalAmountTaken).Amount} {currency}."
            });

            var memberCredit = memberInfo.TotalAmountGiven - memberInfo.TotalAmountTaken;

            pendingTransactionsMemberAsCreditor.ForEach(p =>
            {
              var debtor = retainedDebtors.FirstOrDefault(d => d.Id == p.SenderId);
              var debtorAmount = debtor.TotalAmountTaken - debtor.TotalAmountGiven;

              var senderName = membersWithNames.Single(m => m.Id == p.SenderId).Name;

              if (debtor.Id == p.SenderId)
              {
                var Txt = (debtor.TotalAmountTaken - debtor.TotalAmountGiven - p.Amount).Amount switch
                {
                  0 =>
                    $"{(debtor.TotalAmountGiven == Money.Zero(isoCurrency) ? $"{senderName} paid nothing for the group in {currency}." : $"{senderName}'s total amount paid for the group is {debtor.TotalAmountGiven.Amount} {currency}.")}" +
                    $" The total amount {senderName} received from the group is {debtor.TotalAmountTaken.Amount}, therefore {senderName} is a debtor, with debt of {(debtor.TotalAmountTaken - debtor.TotalAmountGiven).Amount} {currency}. " +
                    //$"This debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency} is paid from {senderName} to {memberName}, reducing {memberName}'s credit to {memberCredit} {currency}. "
                    //$"{senderName} pays this debt of {debtor.TotalAmountTaken - debtor.TotalAmountGiven} {currency} to {memberName}, reducing {memberName}'s credit to {memberCredit} {currency}."
                    $"If {senderName} pays this debt of {(debtor.TotalAmountTaken - debtor.TotalAmountGiven).Amount} {currency} to {memberName}, {((memberCredit -= debtorAmount) == Money.Zero(isoCurrency) ? $"{memberName}'s credit will be cleared." : $"{memberName}'s credit will be reduced to {memberCredit.Amount} {currency}.")}",
                  //+ $"{memberName}'s credit will be reduced to {memberCredit} {currency}.

                  > 0 =>
                    $"{(debtor.TotalAmountGiven == Money.Zero(isoCurrency) ? $"{senderName} paid nothing for the group in {currency}." : $"{senderName}'s total amount paid for the group is {debtor.TotalAmountGiven.Amount} {currency}.")}" +
                    $" The total amount {senderName} received from the group is {debtor.TotalAmountTaken.Amount} {currency}, therefore {senderName} is a debtor, with debt of {(debtor.TotalAmountTaken - debtor.TotalAmountGiven).Amount} {currency}. " +
                    $"If {memberName} takes {p.Amount.Amount} from {senderName}, {memberName}'s credit will be cleared."//.{senderName} reduces their debt to {debtor.TotalAmountTaken - debtor.TotalAmountGiven - p.Amount}{currency} as a result. "
                  ,
                  _ => ""
                };
                explanationTexts.Add(new ExplanationText { Txt = Txt });
              }
            });

            break;

          case (false, true):

            memberInfo = retainedDebtors.FirstOrDefault(d => d.Id == memberId);

            var pendingTransactionsMemberAsDebtor = pendingTransactions.Where(pt => pt.SenderId == memberId && pt.Currency == currency).ToList();

            memberName = membersWithNames.Single(m => m.Id == memberId).Name;

            explanationTexts.Add(new ExplanationText
            {
              Txt = $"{(memberInfo.TotalAmountGiven == Money.Zero(isoCurrency) ? $"{memberName} paid nothing for the group in {currency}." : $"The total amount {memberName} paid for the group is {memberInfo.TotalAmountGiven.Amount}{currency}.")} The total amount {memberName} received from the group is {memberInfo.TotalAmountTaken.Amount} {currency}." +
                    $"As a result {memberName} is a debtor with a debt of {(memberInfo.TotalAmountTaken - memberInfo.TotalAmountGiven).Amount} {currency}."
            });
            var memberDebt = memberInfo.TotalAmountTaken - memberInfo.TotalAmountGiven;

            pendingTransactionsMemberAsDebtor.ForEach(p =>
            {
              var creditor = retainedCreditors.FirstOrDefault(c => c.Id == p.ReceiverId);
              var creditorAmount = creditor.TotalAmountGiven - creditor.TotalAmountTaken;
              var receiverName = membersWithNames.Single(m => m.Id == p.ReceiverId).Name;

              if (creditor.Id == p.ReceiverId)
              {
                var Txt = (creditor.TotalAmountGiven - creditor.TotalAmountTaken - p.Amount).Amount switch
                {
                  0 => $"{receiverName}'s total amount paid for the group is {creditor.TotalAmountGiven.Amount} {currency}. " +
                     $"{(creditor.TotalAmountTaken == Money.Zero(isoCurrency) ? $"{receiverName} did not receive anything from the group in {currency}." : $"The total amount {receiverName} received from the group is {creditor.TotalAmountTaken.Amount} {currency}.")}" +
                     $"As a result {receiverName} is a creditor, with a credit of {(creditor.TotalAmountGiven - creditor.TotalAmountTaken).Amount} {currency}. " +
                     //$"This credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency} is paid from {memberName} to {receiverName}, reducing {memberName}'s debt to {memberDebt} {currency}."
                     //$"{memberName} pays this credit of {creditor.TotalAmountGiven - creditor.TotalAmountTaken} {currency} to {receiverName}, reducing {memberName}'s debt to {memberDebt} {currency}."
                     $"If {memberName} pays this credit of {(creditor.TotalAmountGiven - creditor.TotalAmountTaken).Amount} {currency} to {receiverName}, {((memberDebt -= creditorAmount) == Money.Zero(isoCurrency) ? $"{memberName}'s debt will be cleared." : $"{memberName}'s debt will be reduced to {memberDebt.Amount} {currency}.")}"
                  ,
                  > 0 => $"{receiverName}'s total amount paid for the group is {creditor.TotalAmountGiven.Amount} {currency}. " +
                     $"{(creditor.TotalAmountTaken == Money.Zero(isoCurrency) ? $"{receiverName} did not receive anything from the group in {currency}." : $"The total amount {receiverName} received from the group is {creditor.TotalAmountTaken.Amount} {currency}.")}" +
                     $"As a result {receiverName} is a creditor, with a credit of {(creditor.TotalAmountGiven - creditor.TotalAmountTaken).Amount} {currency}. " +
                     $"If {memberName} pays {p.Amount.Amount} {currency} to {receiverName}, {memberName}'s debt will be cleared.", //and reducing {receiverName}'s credit to {creditor.TotalAmountGiven - creditor.TotalAmountTaken - p.Amount}{currency}.",
                  _ => ""
                };
                explanationTexts.Add(new ExplanationText { Txt = Txt });
              }
            });

            break;

          case (false, false):
            explanationTexts.Add(new ExplanationText
            {
              Txt = ""
            });
            break;
        }
      });
    });
    return explanationTexts;
  }
}