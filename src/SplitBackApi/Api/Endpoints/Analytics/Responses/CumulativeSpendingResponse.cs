using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Analytics.Responses;

public class CumulativeSpendingResponse
{
  public List<decimal> CumulativeSpendingArray { get; set; }
}