using SplitBackApi.Domain;
using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class TransferExtensions
{

  public static TransactionMemberDetail? ToTransactionMemberDetailFromUserId(this Transfer transfer, string userId)
  {
    var isSender = transfer.SenderId == userId;
    var isReceiver = transfer.ReceiverId == userId;
    decimal lent = 0;
    decimal borrowed = 0;
    decimal paid = 0;
    decimal participation = 0;

    if (!isSender && !isReceiver) return null;

    if (isSender) {
      lent = transfer.Amount;
    }

    if (isReceiver) {
      borrowed = transfer.Amount;
    }

    return new TransactionMemberDetail {
      Id = transfer.Id,
      CreatedAt = transfer.CreationTime,
      Description = transfer.Description,
      Lent = lent,
      Borrowed = borrowed,
      UserPaid = paid,
      UserShare = participation,
      IsTransfer = true,
      IsoCode = transfer.IsoCode
    };
  }

}
