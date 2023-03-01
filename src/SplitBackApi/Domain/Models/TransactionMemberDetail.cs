namespace SplitBackApi.Domain;

public class TransactionMemberDetail {
  
  public string Id { get; set; }

  public DateTime TransactionTime { get; set; }

  public string Description { get; set; }
  
  public decimal Lent { get; set; }

  public decimal Borrowed { get; set; }

  public decimal UserPaid { get; set; }

  public decimal UserShare { get; set; }

  public bool IsTransfer { get; set; }

  public string Currency { get; set; }
}