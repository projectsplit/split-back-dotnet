using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class Session : EntityBase {

  public string RefreshToken { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }

  public string Unique { get; set; }
}
