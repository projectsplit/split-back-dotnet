namespace SplitBackApi.Requests;

public class EditTransferRequest {

  public string TransferId { get; set; }

  public string Description { get; set; }

  public string Amount { get; set; }

  public string Currency { get; set; }
  
  public DateTime TransferTime { get; set; }

  public string ReceiverId { get; set; }
  
  public string SenderId { get; set; }
}