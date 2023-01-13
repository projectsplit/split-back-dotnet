using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
  public class ExpenseSpenderDto
  {
    [MaxLength(20)]
    public string SpenderId { get; set; }= null!;
    [MaxLength(29)]
    public string SpenderAmount { get; set; } = null!;
  }