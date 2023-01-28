namespace SplitBackApi.Requests;

public class NewExpenseDto : IExpenseDto {

  public string GroupId { get; set; } = null!;

  public string Description { get; set; } = null!;

  public string Amount { get; set; } = null!;

  public bool SplitEqually { get; set; }

  public string IsoCode { get; set; } = null!;

  //public ICollection<LabelDto> Labels { get; set; } = new List<LabelDto>();

  public ICollection<ParticipantDto> Participants { get; set; }

  public ICollection<SpenderDto> Spenders { get; set; }
}