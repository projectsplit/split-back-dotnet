using SplitBackApi.Domain;
using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class ExpenseExtensions
{

  public static TransactionMemberDetail? ToTransactionMemberDetailFromUserId(this Expense expense, ObjectId userId) {

    bool isSpender = expense.Spenders.ToList().Any(es => es.Id == userId);
    bool isParticipant = expense.Participants.ToList().Any(ep => ep.Id == userId);
    decimal lent = 0;
    decimal borrowed = 0;
    decimal paid = 0;
    decimal participation = 0;

    if(!isSpender && !isParticipant) return null;

    if(isSpender && isParticipant) {

      var spenderAmount = expense.Spenders.ToList().Single(es => es.Id == userId).AmountSpent;
      var participantAmount = expense.Participants.ToList().Single(ep => ep.Id == userId).ContributionAmount;
      lent = spenderAmount - participantAmount;
      paid = spenderAmount;
      participation = participantAmount;
    }

    if (isSpender && !isParticipant) {

      var spenderAmount = expense.Spenders.ToList().Single(es => es.Id == userId).AmountSpent;
      lent = spenderAmount;
      paid = spenderAmount;
    }

    if (!isSpender && isParticipant) {

      var participantAmount = expense.Participants.ToList().Single(ep => ep.Id == userId).ContributionAmount;
      borrowed = participantAmount;
    }

    return new TransactionMemberDetail {
      Id = expense.Id,
      CreatedAt = expense.CreationTime,
      Description = expense.Description,
      Lent = lent,
      Borrowed = borrowed,
      UserPaid = paid,
      UserShare = participation,
      IsTransfer = false,
      IsoCode = expense.IsoCode
    };
  }
}
