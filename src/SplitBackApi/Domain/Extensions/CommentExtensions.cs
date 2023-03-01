using MongoDB.Bson;
using SplitBackApi.Domain;

namespace SplitBackApi.Data.Extensions;

public static class CommentExtensions {
  
  public static PastComment ToPastComment(this Comment comment) {
    
    return new PastComment {
      Id = ObjectId.GenerateNewId().ToString(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      CommentId = comment.Id,
      MemberId = comment.MemberId,
      ParentId = comment.ParentId,
      Text = comment.Text
    };
  }
}