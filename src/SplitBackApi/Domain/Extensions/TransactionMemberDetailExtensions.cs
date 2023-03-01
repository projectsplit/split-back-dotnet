using SplitBackApi.Domain;

namespace SplitBackApi.Extensions;

public static class TransactionMemberDetailExtensions {

  public static TransactionTimelineItem ToTransactionTimelineItem(
    this TransactionMemberDetail transactionMemberDetail,
    decimal totalLentSoFar,
    decimal totalBorrowedSoFar
  ) {
    
    return new TransactionTimelineItem {
      Id = transactionMemberDetail.Id,
      TransactionTime = transactionMemberDetail.TransactionTime,
      Description = transactionMemberDetail.Description,
      Lent = transactionMemberDetail.Lent,
      Borrowed = transactionMemberDetail.Borrowed,
      UserPaid = transactionMemberDetail.UserPaid,
      UserShare = transactionMemberDetail.UserShare,
      IsTransfer = transactionMemberDetail.IsTransfer,
      Currency = transactionMemberDetail.Currency,

      TotalLent = totalLentSoFar,
      TotalBorrowed = totalBorrowedSoFar,
      Balance = totalLentSoFar - totalBorrowedSoFar
    };
  }
}