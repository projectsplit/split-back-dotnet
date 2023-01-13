using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
public class NewExpenseDto : IExpenseDto
{
    public string GroupId { get; set; } = null!;
    [MaxLength(80)]
    public string Description { get; set; } = null!;
    [MaxLength(29)]
    public string Amount { get; set; } = null!;
    public bool SplitEqually { get; set; }
    [MaxLength(3)]
    public string IsoCode { get; set; } = null!;
    //public ICollection<LabelDto> Labels { get; set; } = new List<LabelDto>();
    public ICollection<ParticipantDto> Participants { get; set; } = null!;
    public ICollection<SpenderDto> Spenders { get; set; } = null!;
}