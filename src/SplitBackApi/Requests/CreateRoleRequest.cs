namespace SplitBackApi.Requests;

public class CreateRoleRequest {
  
  public string GroupId { get; set; }
  
  public string Title { get; set; }
  
  public ICollection<int> Permissions { get; set; }
}