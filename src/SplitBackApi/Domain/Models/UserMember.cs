using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

[BsonDiscriminator("User")]
public class UserMember : Member {

  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }
  
}