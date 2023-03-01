namespace SplitBackApi.Domain;

public class TransactionTimelineItem {

  public string Id { get; set; }

  public DateTime TransactionTime { get; set; }

  public string Description { get; set; }

  public decimal Lent { get; set; }

  public decimal Borrowed { get; set; }

  public decimal UserPaid { get; set; }

  public decimal UserShare { get; set; }

  public bool IsTransfer { get; set; }

  public decimal TotalLent { get; set; }

  public decimal TotalBorrowed { get; set; }

  public decimal Balance { get; set; }

  public string Currency { get; set; }
}