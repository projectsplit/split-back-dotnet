using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Session : EntityBase {

  public string RefreshToken { get; set; } = String.Empty;

  [BsonRepresentation(BsonType.ObjectId)] 
  public string UserId { get; set; } = default!;

  public string Unique { get; set; } = String.Empty;
}
