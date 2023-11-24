using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Budgets.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> CreateBudget(
      IBudgetRepository budgetRepository,
      ClaimsPrincipal claimsPrincipal,
      CreateBudgetRequest request,
      BudgetValidator budgetValidator,
      CancellationToken ct)
  {
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var newBudget = new Budget
    {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      UserId = authenticatedUserId,
      Amount = request.Amount,
      Currency = request.Currency,
      BudgetType = request.BudgetType,
      Day = request.Day,
    };

    var validationResult = await budgetValidator.ValidateAsync(newBudget, ct);
    if (validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var createResult = await budgetRepository.Create(newBudget, authenticatedUserId, ct);

    return createResult.IsSuccess ? Results.Ok() : Results.BadRequest(createResult.Error);
  }
}