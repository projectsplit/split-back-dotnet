namespace SplitBackApi.Api.Endpoints.Transfers.Requests;

public class GetTransfersByGroupRequest {

  public string GroupId { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
}