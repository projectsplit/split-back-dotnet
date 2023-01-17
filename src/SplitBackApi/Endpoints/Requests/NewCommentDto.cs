namespace SplitBackApi.Endpoints.Requests;
public class NewCommentDto
{
  public string ExpenseId { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public string? Text { get; set; }

}