using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Role {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; }
  
  public string Title { get; set; } = string.Empty;
  
  public Permissions Permissions { get; set; }
}