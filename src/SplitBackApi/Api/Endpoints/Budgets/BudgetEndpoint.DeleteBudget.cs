
using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Budgets.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> DeleteBudget(
    IBudgetRepository budgetRepository,
    ClaimsPrincipal claimsPrincipal
  )
  {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    var userBudgetFound = await budgetRepository.GetByUserId(authenticatedUserId);

    if (userBudgetFound.IsFailure) return Results.BadRequest($"Budget from user {authenticatedUserId} does not exist");
    await budgetRepository.DeleteByUserId(authenticatedUserId);

    return Results.Ok();

  }

}