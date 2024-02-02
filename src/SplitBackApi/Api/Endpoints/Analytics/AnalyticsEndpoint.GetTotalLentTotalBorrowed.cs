
using System.Security.Claims;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Api.Endpoints.Analytics.Responses;
using SplitBackApi.Data.Repositories.TransferRepository;
using SplitBackApi.Api.Services;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Api.Extensions;


namespace SplitBackApi.Api.Endpoints.Analytics;

public static partial class AnalyticsEndpoints
{
  private static async Task<IResult> GetTotalLentTotalBorrowed(
    IExpenseRepository expenseRepository,
    ITransferRepository transferRepository,
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    HttpRequest request,
    BudgetService budgetService
  )
  {
    var authenticatedUserId = "63ff33b09e4437f07d9d3982"; //claimsPrincipal.GetAuthenticatedUserId();

    var startDateString = request.Query["startDate"].ToString();
    if (string.IsNullOrEmpty(startDateString))
      return Results.BadRequest("startDate is missing, empty or invalid.");

    var endDateString = request.Query["endDate"].ToString();
    if (string.IsNullOrEmpty(startDateString))
      return Results.BadRequest("endDate is missing, empty or invalid.");

    var currency = request.Query["currency"].ToString();
    if (string.IsNullOrEmpty(currency))
      return Results.BadRequest("currency is missing, empty or invalid.");

    var startDate = DateTime.ParseExact(startDateString, "yyyy-MM-dd", null);
    var endDate = DateTime.ParseExact(endDateString, "yyyy-MM-dd", null);

    var groups = await groupRepository.GetGroupsByUserId(authenticatedUserId);
    if (groups.IsNullOrEmpty()) return Results.BadRequest("No groups");

    var totalLentBorowedResult = await budgetService.CalculateCumulativeTotalLentAndBorrowedArray(
      authenticatedUserId,
      groups,
      currency,
      startDate,
      endDate
      );

    if (totalLentBorowedResult.IsFailure) return Results.BadRequest(totalLentBorowedResult.Error);
    var (TotalLent, TotalBorrowed) = totalLentBorowedResult.Value;

    var totalLentArray = TotalLent.Select(c => Math.Round(c.Amount, 2)).ToList(); //TODO might not be the optimal way of returning amounts for all currencies due to rounding
    var totalBorrowedArray = TotalBorrowed.Select(c => Math.Round(c.Amount, 2)).ToList();

    var response = new Dictionary<string, List<decimal>>
    {
        { "totalLent", totalLentArray },
        { "totalBorrowed", totalBorrowedArray }
    };

    return Results.Ok(response);

  }

}
