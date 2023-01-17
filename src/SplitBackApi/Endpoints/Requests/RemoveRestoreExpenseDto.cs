using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public class RemoveRestoreExpenseDto
{
  public string GroupId { get; set; } = null!;
  public string ExpenseId { get; set; } = null!;
  
}