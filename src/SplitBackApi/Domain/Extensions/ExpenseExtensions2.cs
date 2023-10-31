using MongoDB.Bson;
using NMoneys;
using SplitBackApi.Api.Helper;
using SplitBackApi.Domain.Models;


namespace SplitBackApi.Domain.Extensions;

public static class ExpenseExtensions2
{
  public static PastExpense ToPastExpense(this Expense expense)
  {

    return new PastExpense
    {
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

  public static TransactionMemberDetail2 ToTransactionMemberDetailFromUserId2(this Expense expense, string memberId)
  {

    bool isPayer = expense.Payers.Any(p => p.MemberId == memberId);
    bool isParticipant = expense.Participants.ToList().Any(p => p.MemberId == memberId);
    var currency = MoneyHelper.StringToIsoCode(expense.Currency);
    
    Money lent = Money.Zero(currency);
    Money borrowed = Money.Zero(currency);
    Money paid = Money.Zero(currency);
    Money participation = Money.Zero(currency);

    switch (isPayer, isParticipant)
    {
      case (true, true):
        var paymentAmount = new Money(expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal(), currency);
        var participationAmount = new Money(expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal(), currency);
        lent = paymentAmount.Minus(participationAmount);
        paid = paymentAmount;
        participation = participationAmount;
        break;
      case (true, false):
        paymentAmount = new Money(expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal(), currency);
        lent = paymentAmount;
        paid = paymentAmount;
        break;
      case (false, true):
        participationAmount = new Money(expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal(), currency);
        borrowed = participationAmount;
        break;
      default:
        return null;
    }

    return new TransactionMemberDetail2
    {
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