using SplitBackApi.Domain;
using SplitBackApi.Helper;
using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class GroupExtensions {
  
  public static IEnumerable<string> UniqueCurrencyCodes(this Group group) {
    
    var expenseListsByIsoCode = group.Expenses.GroupBy(exp => exp.IsoCode);
    var transferListsByIsoCode = group.Transfers.GroupBy(tr => tr.IsoCode);

    var isoCodeList = new List<string>();
    expenseListsByIsoCode.ToList().ForEach(list => isoCodeList.Add(list.Key));
    transferListsByIsoCode.ToList().ForEach(list => isoCodeList.Add(list.Key));
    var uniqueIsoCodeList = isoCodeList.Distinct();

    return uniqueIsoCodeList;
  }

  public static List<PendingTransaction> PendingTransactions(this Group group) {
    
    var uniqueIsoCodeList = IsoCodeHelper.GetUniqueIsoCodes(group);

    var pendingTransactions = new List<PendingTransaction>();

    uniqueIsoCodeList.ToList().ForEach(currentIsoCode => {
      var participants = new List<Participant>();

      group.Members.ToList().ForEach(member => {
        participants.Add(new Participant(member.UserId, 0m, 0m));
      });

      group.Guests.ToList().ForEach(guest => {
        participants.Add(new Participant(guest.UserId, 0m, 0m));
      });

      group.Expenses.Where(exp => exp.IsoCode == currentIsoCode).ToList().ForEach(expense => {
        expense.Participants.ToList().ForEach(expenseParticipant => {
          participants.Single(p => p.Id == expenseParticipant.Id).TotalAmountTaken += expenseParticipant.ContributionAmount;
        });

        expense.Spenders.ToList().ForEach(expenseSpender => {
          participants.Single(p => p.Id == expenseSpender.Id).TotalAmountGiven += expenseSpender.AmountSpent;
        });
      });

      group.Transfers.Where(tr => tr.IsoCode == currentIsoCode).ToList().ForEach(transfer => {

        participants.Single(p => p.Id == transfer.ReceiverId).TotalAmountTaken += transfer.Amount;

        participants.Single(p => p.Id == transfer.SenderId).TotalAmountGiven += transfer.Amount;
      });

      var debtors = new Queue<Participant>();
      var creditors = new Queue<Participant>();

      participants.ForEach(p => {

        switch(p.TotalAmountGiven - p.TotalAmountTaken) {

          case < 0:
            debtors.Enqueue(p);
            break;

          case > 0:
            creditors.Enqueue(p);
            break;
        }
      });

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
              IsoCode = currentIsoCode,
            });

            creditors.Enqueue(poppedCreditor with { TotalAmountTaken = poppedCreditor.TotalAmountTaken + debt });
            break;

          case > 0:
            pendingTransactions.Add(new PendingTransaction {
              SenderId = poppedDebtor.Id,
              ReceiverId = poppedCreditor.Id,
              Amount = credit,
              IsoCode = currentIsoCode,
            });

            debtors.Enqueue(poppedDebtor with { TotalAmountGiven = poppedDebtor.TotalAmountGiven + credit });
            break;

          case 0:
            pendingTransactions.Add(new PendingTransaction {
              SenderId = poppedDebtor.Id,
              ReceiverId = poppedCreditor.Id,
              Amount = credit, //credit == debt
              IsoCode = currentIsoCode,
            });
            break;
        }
      }
    });

    return pendingTransactions;
  }

  public static void AddAsCreatorAndMember(this Group group, User user) {
    // group.Members.Add(user.Id);
    // group.CreatorId = user.Id;
  }

  public static Dictionary<string, List<TransactionTimelineItem>> GetTransactionHistory(this Group group, ObjectId authedUserId) {
    
    var userId = authedUserId;//ObjectId.Parse("63b7f4db07148f61a575a3bb");//this is going to be the authorized user's Id.
    var uniqueIsoCodeList = group.UniqueCurrencyCodes();
    var transactionTimelineForEachCurrency = new Dictionary<string, List<TransactionTimelineItem>>();

    // Loop currencies used
    foreach(var currencyCode in uniqueIsoCodeList) {
      // New list of TransactionMemberDetail
      var transactionMemberDetails = new List<TransactionMemberDetail>();

      // Loop all expenses & add to list
      foreach(var expense in group.Expenses.Where(exp => exp.IsoCode == currencyCode)) {
        var transactionMemberDetail = expense.ToTransactionMemberDetailFromUserId(userId);

        if(transactionMemberDetail is not null) {
          transactionMemberDetails.Add(transactionMemberDetail);
        }
      };

      // Loop all transfers & add to list
      foreach(var transfer in group.Transfers.Where(exp => exp.IsoCode == currencyCode)) {
        var transactionMemberDetail = transfer.ToTransactionMemberDetailFromUserId(userId);

        if(transactionMemberDetail is not null) {
          transactionMemberDetails.Add(transactionMemberDetail);
        }
      };

      // Sort list
      var sortedTransactionMemberDetails = transactionMemberDetails.OrderBy(h => h.CreatedAt);

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

      transactionTimelineForEachCurrency.Add(currencyCode, transactionTimelineForCurrency);
    };
    
    return transactionTimelineForEachCurrency;
  }
}

public record Participant {
  
  public Participant(ObjectId id, decimal totalAmountGiven, decimal totalAmountTaken) {
    
    Id = id;
    TotalAmountGiven = totalAmountGiven;
    TotalAmountTaken = totalAmountTaken;
  }
  
  public ObjectId Id { get; set; }
  
  public decimal TotalAmountGiven { get; set; }
  
  public decimal TotalAmountTaken { get; set; }
}
