namespace SplitBackApi.Requests;

public class RemoveRoleFromUserRequest {
  
  public string GroupId { get; set; }
  
  public string RoleId { get; set; }
  
  public string UserId { get; set; }
}