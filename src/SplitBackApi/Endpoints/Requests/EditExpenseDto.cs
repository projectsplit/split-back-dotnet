namespace SplitBackApi.Endpoints.Requests;
public class EditExpenseDto : IExpenseDto
{
  public string ExpenseId { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string Amount { get; set; } = null!;
  public bool SplitEqually { get; set; }
  public string IsoCode { get; set; } = null!;
  //public ICollection<LabelDto> Labels { get; set; } = new List<LabelDto>();
  public ICollection<ParticipantDto> Participants { get; set; } = null!;
  public ICollection<SpenderDto> Spenders { get; set; } = null!;
}