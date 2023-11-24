using NMoneys;

namespace SplitBackApi.Domain.Models;

public record TransactionMember2 {

  public TransactionMember2(string id, Money totalAmountGiven, Money totalAmountTaken) {

    Id = id;
    TotalAmountGiven = totalAmountGiven;
    TotalAmountTaken = totalAmountTaken;
  }

  public string Id { get; set; }

  public Money TotalAmountGiven { get; set; }

  public Money TotalAmountTaken { get; set; }
}

public class TransactionMemberWrapper2 {
  public TransactionMemberWrapper2(TransactionMember2 member, Money remainder) {
    Member = member;
    Remainder = remainder;
  }

  public TransactionMember2 Member { get; set; }
  public Money Remainder { get; set; }
}