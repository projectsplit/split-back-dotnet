namespace SplitBackApi.Requests;

public class CreateCommentRequest {
  
  public string GroupId { get; set; }
  
  public string ParentId { get; set; }
  
  public string Text { get; set; }
}