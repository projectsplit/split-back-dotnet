using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;
[BsonKnownTypes(typeof(JwtAuthSession), typeof(ExternalAuthSession))]
public class Session : EntityBase {

  public string RefreshToken { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }

  
}
