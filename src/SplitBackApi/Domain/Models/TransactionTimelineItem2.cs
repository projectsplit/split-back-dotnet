using NMoneys;

namespace SplitBackApi.Domain.Models;

public class TransactionTimelineItem2 {

  public string Id { get; set; }

  public DateTime TransactionTime { get; set; }

  public string Description { get; set; }

  public Money Lent { get; set; }

  public Money Borrowed { get; set; }

  public Money UserPaid { get; set; }

  public Money UserShare { get; set; }

  public bool IsTransfer { get; set; }

  public Money TotalLent { get; set; }

  public Money TotalBorrowed { get; set; }

  public Money Balance { get; set; }

  public string Currency { get; set; }
}