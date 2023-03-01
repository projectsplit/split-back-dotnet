namespace SplitBackApi.Api.Endpoints.Comments.Requests;

public class EditCommentRequest {
  
  public string CommentId { get; set; }
  
  public string Text { get; set; }
}