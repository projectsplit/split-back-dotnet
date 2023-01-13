using MongoDB.Bson;
namespace SplitBackApi.Domain;

public class Transfer : EntityBase {
  
  public string Description { get; set; } = string.Empty;
  
  public decimal Amount { get; set; }
  
  public string IsoCode { get; set; } = string.Empty;
  
  public DateTime TransferTime { get; set; }
  
  public ObjectId SenderId { get; set; }
  
  public ObjectId ReceiverId { get; set; }
  
  public ICollection<TransferSnapshot> History { get; set; } = new List<TransferSnapshot>();

  public bool IsDeleted { get; set; } = false;
}
