using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Transfer : EntityBase {
  
  public string Description { get; set; } = string.Empty;
  
  public decimal Amount { get; set; }
  
  public string IsoCode { get; set; } = string.Empty;
  
  public DateTime TransferTime { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string SenderId { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string ReceiverId { get; set; }
  
  public ICollection<TransferSnapshot> History { get; set; } = new List<TransferSnapshot>();
}
