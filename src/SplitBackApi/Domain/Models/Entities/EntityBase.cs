using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class EntityBase {
  
  [BsonRepresentation(BsonType.ObjectId)] 
  public string Id { get; set; } = default!;
  
  public DateTime CreationTime { get; set; } = DateTime.UtcNow;
  
  public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
}
