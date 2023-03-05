namespace SplitBackApi.Api.Endpoints.Groups.Requests;

public class RemoveGuestRequest {

  public string GroupId { get; set; }
  
  public string MemberId{ get; set; }

}