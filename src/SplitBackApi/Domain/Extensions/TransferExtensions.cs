using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Extensions;

namespace SplitBackApi.Data.Extensions;

public static class TransferExtensions {
  
  public static PastTransfer ToPastTransfer(this Transfer transfer) {
    
    return new PastTransfer {
      Id = ObjectId.GenerateNewId().ToString(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      TransferId = transfer.Id,
      Amount = transfer.Amount,
      Currency = transfer.Currency,
      Description = transfer.Description,
      GroupId = transfer.GroupId,
      ReceiverId = transfer.ReceiverId,
      SenderId = transfer.SenderId,
      TransferTime = transfer.TransferTime
    };
  }
  
  public static TransactionMemberDetail ToTransactionMemberDetailFromUserId(
    this Transfer transfer,
    string memberId
  ) {

    var isSender = transfer.SenderId == memberId;
    var isReceiver = transfer.ReceiverId == memberId;
    decimal lent = 0;
    decimal borrowed = 0;
    decimal paid = 0;
    decimal participation = 0;

    if(!isSender && !isReceiver) return null;

    if(isSender) {
      lent = transfer.Amount.ToDecimal();
    }

    if(isReceiver) {
      borrowed = transfer.Amount.ToDecimal();
    }

    return new TransactionMemberDetail {
      Id = transfer.Id,
      TransactionTime = transfer.TransferTime,
      Description = transfer.Description,
      Lent = lent,
      Borrowed = borrowed,
      UserPaid = paid,
      UserShare = participation,
      IsTransfer = true,
      Currency = transfer.Currency
    };
  }
}