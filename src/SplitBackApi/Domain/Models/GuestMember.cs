using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

[BsonDiscriminator("Guest")]
public class GuestMember : Member {
  
  public string Name { get; set; }
}