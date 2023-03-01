using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; }

  public DateTime CreationTime { get; set; }

  public DateTime LastUpdateTime { get; set; }
}
