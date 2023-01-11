using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class User : EntityBase {

  public string Nickname { get; set; } = String.Empty;

  public string Email { get; set; } = String.Empty;

  [BsonRepresentation(BsonType.ObjectId)] 
  public ICollection<string> Groups { get; set; } = new List<string>();
}
