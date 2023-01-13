using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
  public class ExpenseParticipantDto
  {
    [MaxLength(20)]
    public string ParticipantId { get; set; } = null!;
    [MaxLength(29)]
    public string ContributionAmount { get; set; } = null!;
  }