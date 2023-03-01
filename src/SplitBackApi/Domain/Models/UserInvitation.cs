using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

[BsonDiscriminator("Replacement")]
public class ReplacementInvitation : Invitation {
  
  public string MemberId { get; set; }
}