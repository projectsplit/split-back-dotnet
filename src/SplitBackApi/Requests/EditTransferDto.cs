namespace SplitBackApi.Requests;

public class EditTransferDto : ITransferDto {
  
  public string TransferId { get; set; }
  
  public string GroupId { get; set; }
  
  public string Description { get; set; }
  
  public string IsoCode { get; set; }
  
  public string Amount { get; set; }

  public string SenderId { get; set; }

  public string ReceiverId { get; set; }
}