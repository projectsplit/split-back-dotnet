namespace SplitBackApi.Api.Endpoints.Groups.Responses;
public class GroupsAggregatedSummaryResponse {
  public Dictionary<string, decimal> UserIsOwedAmounts { get; set; }
  public Dictionary<string, decimal> UserOwesAmounts { get; set; }
}