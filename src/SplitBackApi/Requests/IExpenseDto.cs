namespace SplitBackApi.Requests;

public interface IExpenseDto {
  
  public string GroupId { get; set; }

  public string Description { get; set; }
  
  public string Amount { get; set; }
  
  public bool SplitEqually { get; set; }

  public string IsoCode { get; set; }
  
  //public ICollection<LabelDto> Labels { get; set; }
  
  public ICollection<ParticipantDto> Participants { get; set; }
  
  public ICollection<SpenderDto> Spenders { get; set; }
}