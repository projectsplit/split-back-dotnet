using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public interface IExpenseDto
{
  public string GroupId { get; set; }
  [MaxLength(80)]
  public string Description { get; set; }
  [MaxLength(29)]
  public string Amount { get; set; }
  public bool SplitEqually { get; set; }
  [MaxLength(3)]
  public string IsoCode { get; set; }
  //public ICollection<LabelDto> Labels { get; set; }
  public ICollection<ParticipantDto> Participants { get; set; }
  public ICollection<SpenderDto> Spenders { get; set; }

}