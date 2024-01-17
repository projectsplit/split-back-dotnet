namespace SplitBackApi.Api.Endpoints.Analytics;

public static partial class AnalyticsEndpoints
{
  public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
  {
    var analyticsGroup = app
        .MapGroup("/analytics")
        .WithTags("Analytics")
        .AllowAnonymous();

    analyticsGroup.MapGet("/cumulativespending", GetCumulativeSpending);
    analyticsGroup.MapGet("/totallentborrowed", GetTotalLentTotalBorrowed);

  }
}