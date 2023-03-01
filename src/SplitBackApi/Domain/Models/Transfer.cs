using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Transfer : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; } = string.Empty;
  
  public DateTime TransferTime { get; set; }

  public string Description { get; set; } = string.Empty;
  
  public string Amount { get; set; } = string.Empty;
  
  public string Currency { get; set; } = string.Empty;
  
  public string SenderId { get; set; } = string.Empty;
  
  public string ReceiverId { get; set; } = string.Empty;
}