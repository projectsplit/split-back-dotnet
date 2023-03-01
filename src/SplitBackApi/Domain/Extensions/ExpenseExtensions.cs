using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Extensions;

namespace SplitBackApi.Data.Extensions;

public static class ExpenseExtensions {
  
  public static PastExpense ToPastExpense(this Expense expense) {
    
    return new PastExpense {
      Id = ObjectId.GenerateNewId().ToString(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      ExpenseId = expense.Id,

      Amount = expense.Amount,
      Description = expense.Description,
      Currency = expense.Currency,
      ExpenseTime = expense.ExpenseTime,
      GroupId = expense.GroupId,
      Labels = expense.Labels,
      Participants = expense.Participants,
      Payers = expense.Payers
    };
  }

  public static TransactionMemberDetail? ToTransactionMemberDetailFromUserId(this Expense expense, string memberId) {

    bool isPayer = expense.Payers.Any(p => p.MemberId == memberId);
    bool isParticipant = expense.Participants.ToList().Any(p => p.MemberId == memberId);
    decimal lent = 0;
    decimal borrowed = 0;
    decimal paid = 0;
    decimal participation = 0;

    if(!isPayer && !isParticipant) return null;

    if(isPayer && isParticipant) {

      var paymentAmount = expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal();
      var participationAmount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();
      lent = paymentAmount - participationAmount;
      paid = paymentAmount;
      participation = participationAmount;
    }

    if(isPayer && !isParticipant) {

      var paymentAmount = expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal();
      lent = paymentAmount;
      paid = paymentAmount;
    }

    if(!isPayer && isParticipant) {

      var participationAmount = expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal();
      borrowed = participationAmount;
    }

    return new TransactionMemberDetail {
      Id = expense.Id,
      TransactionTime = expense.ExpenseTime,
      Description = expense.Description,
      Lent = lent,
      Borrowed = borrowed,
      UserPaid = paid,
      UserShare = participation,
      IsTransfer = false,
      Currency = expense.Currency
    };
  }
}