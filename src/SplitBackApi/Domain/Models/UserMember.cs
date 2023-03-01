using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

[BsonDiscriminator("User")]
public class UserMember : Member {

  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }
}