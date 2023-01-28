namespace SplitBackApi.Requests;

public class EditExpenseDto : IExpenseDto {
  
  public string ExpenseId { get; set; }
  
  public string GroupId { get; set; }
  
  public string Description { get; set; }
  
  public string Amount { get; set; }
  
  public bool SplitEqually { get; set; }
  
  public string IsoCode { get; set; }
  
  //public ICollection<LabelDto> Labels { get; set; } = new List<LabelDto>();
  
  public ICollection<ParticipantDto> Participants { get; set; }
  
  public ICollection<SpenderDto> Spenders { get; set; }
}