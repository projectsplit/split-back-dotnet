using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public class RemoveRestoreTransferDto
{
  [MaxLength(20)]
  public string GroupId { get; set; } = null!;
  [MaxLength(20)]
  public string TransferId { get; set; } = null!;
}