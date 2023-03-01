using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class PastComment : EntityBase {

  [BsonRepresentation(BsonType.ObjectId)]
  public string CommentId { get; set; }
  
  [BsonRepresentation(BsonType.ObjectId)]
  public string ParentId { get; set; }
  
  public string MemberId { get; set; }

  public string Text { get; set; } = string.Empty;
}