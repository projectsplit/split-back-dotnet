using MongoDB.Bson;

namespace SplitBackApi.Domain;

public class Comment : EntityBase {
    
  public ObjectId CommenterId { get; set; }
  
  public string Text { get; set; } = string.Empty;
}
