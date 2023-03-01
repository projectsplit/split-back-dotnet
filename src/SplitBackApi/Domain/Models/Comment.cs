using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain.Models;

public class Comment : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string ParentId { get; set; }
  
  public string MemberId { get; set; }

  public string Text { get; set; }
}