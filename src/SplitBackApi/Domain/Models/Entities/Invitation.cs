using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Invitation : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string Inviter { get; set; }

  public string Code { get; set; }

  [BsonRepresentation(BsonType.ObjectId)]
  public string GroupId { get; set; }
}