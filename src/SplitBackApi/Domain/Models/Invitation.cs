using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

[BsonKnownTypes(typeof(ReplacementInvitation))]
[BsonDiscriminator("Basic")]
public class Invitation : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string InviterId { get; set; }

  public string Code { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; }
  
  public DateTime ExpirationTime { get; set; }
  
  public int NumberOfUses { get; set; }
  
  public ICollection<InvitationUse> Uses { get; set; } = new List<InvitationUse>();
}