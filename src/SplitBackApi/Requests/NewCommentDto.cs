namespace SplitBackApi.Requests;

public class NewCommentDto : GroupOperationRequestBase {
  
  public string ExpenseId { get; set; }
  
  public string Text { get; set; }
}