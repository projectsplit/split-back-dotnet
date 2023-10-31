using NMoneys;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Domain.Extensions;

public static class TransactionMemberDetailExtensions2 {

  public static TransactionTimelineItem2 ToTransactionTimelineItem2(
    this TransactionMemberDetail2 transactionMemberDetail,
    Money totalLentSoFar,
    Money totalBorrowedSoFar
  ) {
    
    return new TransactionTimelineItem2 {
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