using MongoDB.Bson;
using NMoneys;
using SplitBackApi.Api.Helper;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Extensions;

public static class TransferExtensions2
{

  public static PastTransfer ToPastTransfer2(this Transfer transfer)
  {

    return new PastTransfer
    {
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

  public static TransactionMemberDetail2 ToTransactionMemberDetailFromUserId2(
    this Transfer transfer,
    string memberId
  )
  {
    var isSender = transfer.SenderId == memberId;
    var isReceiver = transfer.ReceiverId == memberId;
    var currency = MoneyHelper.StringToIsoCode(transfer.Currency);

    Money lent = Money.Zero(currency);
    Money borrowed = Money.Zero(currency);
    Money paid = Money.Zero(currency);
    Money participation = Money.Zero(currency);

    switch (isSender, isReceiver)
    {
      case (true, false):
        lent = new Money(transfer.Amount.ToDecimal(), currency);
        break;
      case (false, true):
        borrowed = new Money(transfer.Amount.ToDecimal(), currency);
        break;
      case (false, false):
        return null;
    }

    return new TransactionMemberDetail2
    {
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