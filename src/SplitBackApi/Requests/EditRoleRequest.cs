namespace SplitBackApi.Requests;

public class EditRoleRequest {
  
  public string RoleId { get; set; }
  
  public string GroupId { get; set; }
  
  public string Title { get; set; }
  
  public ICollection<int> Permissions { get; set; }
}