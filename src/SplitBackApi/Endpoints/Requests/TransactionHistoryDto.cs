using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public class TransactionHistoryDto
{
  [MaxLength(80)]
  public string GroupId { get; set; } = null!;
}