using MongoDB.Bson;
using NMoneys;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Extensions;

public static class ExpenseExtensions2
{
  public static PastExpense ToPastExpense2(this Expense expense)
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
    var isPayer = expense.Payers.Any(p => p.MemberId == memberId);
    var isParticipant = expense.Participants.ToList().Any(p => p.MemberId == memberId);
    var currency = expense.Currency.StringToIsoCode();

    var lent = Money.Zero(currency);
    var borrowed = Money.Zero(currency);
    var paid = Money.Zero(currency);
    var participation = Money.Zero(currency);

    if (isPayer && isParticipant)
    {
      var paymentAmount = new Money(expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal(), currency);
      var participationAmount = new Money(expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal(), currency);
      lent = paymentAmount.Minus(participationAmount);
      paid = paymentAmount;
      participation = participationAmount;
    };

    if (isPayer && !isParticipant)
    {
      var paymentAmount = new Money(expense.Payers.Single(p => p.MemberId == memberId).PaymentAmount.ToDecimal(), currency);
      lent = paymentAmount;
      paid = paymentAmount;
    };

    if (!isPayer && isParticipant)
    {
      var participationAmount = new Money(expense.Participants.Single(p => p.MemberId == memberId).ParticipationAmount.ToDecimal(), currency);
      borrowed = participationAmount;
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