namespace SplitBackApi.Requests;

using SplitBackApi.Domain;

public class CreateGroupDto {
  
  public string Title { get; set; }
  
  public ICollection<Label>? GroupLabels { get; set; }
}