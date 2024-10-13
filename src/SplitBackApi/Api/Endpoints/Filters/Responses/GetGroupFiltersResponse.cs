namespace SplitBackApi.Api.Endpoints.Filters.Responses;

public class FilteredMember
{
  public string MemberId { get; set; }
  public string Value { get; set; }
}

public class GetGroupFiltersResponse
{
  public ICollection<FilteredMember> Payers { get; set; }
  public ICollection<FilteredMember> Participants { get; set; }
  public ICollection<FilteredMember> Senders { get; set; }
  public ICollection<FilteredMember> Receivers { get; set; }
  //   public DateTime Before { get; set; }
  //   public DateTime After { get; set; }
}