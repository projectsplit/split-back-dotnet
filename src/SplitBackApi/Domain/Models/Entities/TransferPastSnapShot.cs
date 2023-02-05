using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class TransferSnapshot : EntityBase {

  public string Description { get; set; } = string.Empty;
  
  public decimal Amount { get; set; }
  
  public string IsoCode { get; set; } = string.Empty;
  
  public DateTime ModifiedAt { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string SenderId { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string ReceiverId { get; set; }

  public bool IsDeleted { get; set; }
}
