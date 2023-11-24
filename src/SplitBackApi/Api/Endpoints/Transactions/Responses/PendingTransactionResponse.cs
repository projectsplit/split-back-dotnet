namespace SplitBackApi.Api.Endpoints.Transactions.Responses;

public class PendingTransactionResponse {

  public string SenderId { get; set; }
  public string ReceiverId { get; set; }
  public string SenderName { get; set; }
  public string ReceiverName { get; set; }
  public decimal Amount { get; set; }
  public string Currency { get; set; }

}
