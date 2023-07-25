namespace SplitBackApi.Api.Endpoints.Groups.Responses;
public class GroupsAggregatedSummaryResponse {
  public int numberOfGroups { get; set; }
  public Dictionary<string, decimal> UserIsOwedAmounts { get; set; }
  public Dictionary<string, decimal> UserOwesAmounts { get; set; }
}