namespace SplitBackApi.Api.Endpoints.Permissions.Requests;

public class EditPermissionsRequest {
  
  public string GroupId { get; set; }
  
  public string MemberId { get; set; }
  
  public Domain.Models.Permissions Permissions { get; set; }
}