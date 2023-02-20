using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace SplitBackApi.Domain;

[BsonKnownTypes(typeof(UserMember), typeof(GuestMember))]
public class Member : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public ICollection<string> Roles { get; set; } = new List<string>();

}

//[BsonDiscriminator("100")]
public class UserMember : Member {
  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }
}

public class GuestMember : Member {
  public string Name { get; set; }
}

