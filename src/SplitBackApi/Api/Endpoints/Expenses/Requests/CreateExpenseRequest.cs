namespace SplitBackApi.Api.Endpoints.Expenses.Requests;

public class CreateExpenseRequest {

  public string GroupId { get; set; }

  public string Description { get; set; }

  public string Amount { get; set; }

  public string Currency { get; set; }
  
  public DateTime ExpenseTime { get; set; }

  public ICollection<string> Labels { get; set; } = new List<string>();

  public ICollection<CreateExpenseRequestParticipant> Participants { get; set; }

  public ICollection<CreateExpenseRequestPayer> Payers { get; set; }
}