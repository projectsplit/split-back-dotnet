namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class RemoveExpenseRequest {

  public string ExpenseId { get; set; }
  public string GroupId { get; set; }
}