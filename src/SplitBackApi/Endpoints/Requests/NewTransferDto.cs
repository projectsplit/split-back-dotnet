using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;

public class NewTransferDto : ITransferDto
{
  public string GroupId { get; set; } = null!;
  [MaxLength(80)]
  public string? Description { get; set; }
  public string IsoCode { get; set; } = null!;
  [MaxLength(29)]
  public string Amount { get; set; } = null!;
  [MaxLength(30)]
  public string SenderId { get; set; } = null!;
  [MaxLength(30)]
  public string ReceiverId { get; set; } = null!;
}