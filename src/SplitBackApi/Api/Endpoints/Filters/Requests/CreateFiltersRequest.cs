namespace SplitBackApi.Api.Endpoints.Filters.Requests;
public class CreateFiltersRequest
{
  public string GroupId { get; set; }
  public ICollection<string> PayersIds { get; set; }
  public ICollection<string> ParticipantsIds { get; set; }
  public ICollection<string> SendersIds { get; set; }
  public ICollection<string> ReceiversIds { get; set; }
  public DateTime Before { get; set; }
  public DateTime After { get; set; }
}