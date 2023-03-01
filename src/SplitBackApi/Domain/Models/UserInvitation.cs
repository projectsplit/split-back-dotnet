using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

[BsonDiscriminator("Replacement")]
public class ReplacementInvitation : Invitation {
  
  public string MemberId { get; set; }
}