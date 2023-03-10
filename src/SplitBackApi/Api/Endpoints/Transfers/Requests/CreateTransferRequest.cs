namespace SplitBackApi.Api.Endpoints.Transfers.Requests;

public class CreateTransferRequest {

  public string GroupId { get; set; }

  public string Description { get; set; }

  public string Amount { get; set; }

  public string Currency { get; set; }
  
  public DateTime TransferTime { get; set; }

  public string ReceiverId { get; set; }
  
  public string SenderId { get; set; }
}