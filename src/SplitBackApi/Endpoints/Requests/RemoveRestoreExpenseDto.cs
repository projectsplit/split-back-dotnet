using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public class RemoveRestoreExpenseDto
{
  [MaxLength(20)]
  public string GroupId { get; set; } = null!;
  [MaxLength(20)]
  public string ExpenseId { get; set; } = null!;
  public bool Remove { get; set; }
}