using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

[BsonKnownTypes(typeof(UserInvitation), typeof(GuestInvitation))]

public class Invitation : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string Inviter { get; set; }

  public string Code { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; }
}

public class UserInvitation : Invitation {

}

public class GuestInvitation : Invitation {

  [BsonRepresentation(BsonType.ObjectId)]
  public string GuestId { get; set; }
}
