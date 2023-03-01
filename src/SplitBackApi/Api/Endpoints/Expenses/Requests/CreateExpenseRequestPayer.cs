namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class CreateExpenseRequestPayer {

  public string MemberId { get; set; }

  public string PaymentAmount { get; set; }
}