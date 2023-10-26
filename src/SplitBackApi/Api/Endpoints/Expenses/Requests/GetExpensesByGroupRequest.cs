namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class GetExpensesByGroupRequest {

  public string GroupId { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }

}