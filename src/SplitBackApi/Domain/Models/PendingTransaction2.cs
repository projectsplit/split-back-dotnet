using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NMoneys;

namespace SplitBackApi.Domain.Models;

public class PendingTransaction2 {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string SenderId { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string ReceiverId { get; set; }
  
  public Money Amount { get; set; }

  public string Currency { get; set; }
}