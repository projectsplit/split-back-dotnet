using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SplitBackApi.Domain;

public class Comment : EntityBase {
    
  [BsonRepresentation(BsonType.ObjectId)]
  public string CommentorId { get; set; }
  
  public string Text { get; set; } = string.Empty;
}
