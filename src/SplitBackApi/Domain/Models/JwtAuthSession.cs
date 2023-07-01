using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

[BsonDiscriminator("JwtAuth")]
public class JwtAuthSession : Session {
  public string Unique { get; set; }
}