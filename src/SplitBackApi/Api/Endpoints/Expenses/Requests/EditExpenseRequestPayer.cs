namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class EditExpenseRequestPayer {

  public string MemberId { get; set; }

  public string PaymentAmount { get; set; }
}