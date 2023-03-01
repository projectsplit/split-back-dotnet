using SplitBackApi.Domain;

namespace SplitBackApi.Requests;

public class EditPermissionsRequest {
  
  public string GroupId { get; set; }
  
  public string MemberId { get; set; }
  
  public Permissions Permissions { get; set; }
}