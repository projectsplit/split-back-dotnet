using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Member {
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public ICollection<string> Roles { get; set; } = new List<string>();

  //public ObjectId InvitedBy { get; set; }
}