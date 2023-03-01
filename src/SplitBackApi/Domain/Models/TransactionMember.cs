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