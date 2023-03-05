namespace SplitBackApi.Api.Endpoints.Comments.Requests;

public class DeleteCommentRequest {
  public string GroupId { get; set; }
  public string CommentId { get; set; }
}