namespace SplitBackApi.Endpoints.Requests;
public class NewCommentDto : X {
  public string ExpenseId { get; set; } = null!;
  //public string GroupId { get; set; } = null!;
  public string? Text { get; set; }

}

public class X {
  public string GroupId { get; set; }
}
