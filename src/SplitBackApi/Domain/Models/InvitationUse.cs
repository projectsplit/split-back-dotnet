using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class InvitationUse {

  [BsonRepresentation(BsonType.ObjectId)]
  public string UserId { get; set; }

  public DateTime UseTime { get; set; }
}