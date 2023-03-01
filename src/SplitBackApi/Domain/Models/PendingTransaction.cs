using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class PendingTransaction {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string SenderId { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string ReceiverId { get; set; }
  
  public decimal Amount { get; set; }

  public string Currency { get; set; }
}