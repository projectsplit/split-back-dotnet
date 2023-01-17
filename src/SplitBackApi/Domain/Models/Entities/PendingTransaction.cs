namespace SplitBackApi.Domain;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

public class PendingTransaction
{
  public ObjectId SenderId { get; set; }
  public ObjectId ReceiverId { get; set; }
  public decimal Amount { get; set; }
  [MaxLength(3)]
  public string IsoCode { get; set; } = null!;
}
