namespace SplitBackApi.Domain.Models;

public record TransactionMember {

  public TransactionMember(string id, decimal totalAmountGiven, decimal totalAmountTaken) {

    Id = id;
    TotalAmountGiven = totalAmountGiven;
    TotalAmountTaken = totalAmountTaken;
  }

  public string Id { get; set; }

  public decimal TotalAmountGiven { get; set; }

  public decimal TotalAmountTaken { get; set; }
}

public class TransactionMemberWrapper {
  public TransactionMemberWrapper(TransactionMember member, decimal remainder) {
    Member = member;
    Remainder = remainder;
  }

  public TransactionMember Member { get; set; }
  public decimal Remainder { get; set; }
}