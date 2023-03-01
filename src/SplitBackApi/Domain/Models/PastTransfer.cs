using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class PastTransfer : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string TransferId { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; }
  
  public DateTime TransferTime { get; set; }

  public string Description { get; set; }
  
  public string Amount { get; set; }
  
  public string Currency { get; set; }
  
  public string SenderId { get; set; }
  
  public string ReceiverId { get; set; }
}