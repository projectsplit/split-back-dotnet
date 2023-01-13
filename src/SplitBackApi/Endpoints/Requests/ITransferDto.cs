using System.ComponentModel.DataAnnotations;
namespace SplitBackApi.Endpoints.Requests;
public interface ITransferDto
{
  public string GroupId { get; set; }
  [MaxLength(80)]
  public string? Description { get; set; }
  public string IsoCode { get; set; }
  [MaxLength(29)]
  public string Amount { get; set; } 
  [MaxLength(30)]
  public string SenderId { get; set; } 
  [MaxLength(30)]
  public string ReceiverId { get; set; } 
}