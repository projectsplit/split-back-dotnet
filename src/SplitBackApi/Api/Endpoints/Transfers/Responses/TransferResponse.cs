namespace SplitBackApi.Api.Endpoints.Transfers.Responses;

public class TransferResponse : Domain.Models.Transfer {
  public string SenderName { get; set; }
  public string ReceiverName { get; set; }

}