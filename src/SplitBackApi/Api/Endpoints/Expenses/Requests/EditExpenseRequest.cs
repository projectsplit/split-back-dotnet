namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class EditExpenseRequest {

  public string ExpenseId { get; set; }

  public string Description { get; set; }

  public string Amount { get; set; }

  public string Currency { get; set; }
  
  public DateTime ExpenseTime { get; set; }

  public ICollection<string> Labels { get; set; } = new List<string>();

  public ICollection<EditExpenseRequestParticipant> Participants { get; set; }

  public ICollection<EditExpenseRequestPayer> Payers { get; set; }
}