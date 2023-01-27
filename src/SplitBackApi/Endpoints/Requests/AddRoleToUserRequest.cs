namespace SplitBackApi.Endpoints.Requests;


public class AddRoleToUserRequest {
  public string GroupId { get; set; }
  public string RoleId { get; set; }
  public string UserId { get; set; }

}