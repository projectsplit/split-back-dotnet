using MongoDB.Bson;
namespace SplitBackApi.Domain;
public class TransactionTimelineItem
{

  public ObjectId Id { get; set; }

  public DateTime CreatedAt { get; set; }

  public string Description { get; set; } = String.Empty;

  public decimal Lent { get; set; }

  public decimal Borrowed { get; set; }

  public decimal UserPaid { get; set; }

  public decimal UserShare { get; set; }

  public bool IsTransfer { get; set; }

  public decimal TotalLent { get; set; }

  public decimal TotalBorrowed { get; set; }

  public decimal Balance { get; set; }

  public string IsoCode { get; set; } = String.Empty;
}