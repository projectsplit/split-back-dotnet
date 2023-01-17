using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
  public class SpenderDto
  {
    [MaxLength(20)]
    public string SpenderId { get; set; }= null!;
    [MaxLength(29)]
    public string AmountSpent { get; set; } = null!;
  }