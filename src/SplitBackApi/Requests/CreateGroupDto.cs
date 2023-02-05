namespace SplitBackApi.Requests;

public class CreateGroupDto {

  public string Title { get; set; }
  
  public string BaseCurrencyCode { get; set; }
  
  public ICollection<LabelDto>? GroupLabels { get; set; }
}