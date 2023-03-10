namespace SplitBackApi.Api.Endpoints.Groups.Requests;

public class CreateGroupRequest {

  public string Title { get; set; }
  
  public string BaseCurrencyCode { get; set; }
  
  public ICollection<CreateGroupRequestLabel> Labels { get; set; }
}