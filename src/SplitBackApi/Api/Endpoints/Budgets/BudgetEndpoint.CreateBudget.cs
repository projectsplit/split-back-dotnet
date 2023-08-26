
using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Budgets.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.BudgetRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Budgets;

public static partial class BudgetsEndpoints
{
  private static async Task<IResult> CreateBudget(
    IBudgetRepository budgetRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateBudgetRequest request
  )
  {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    var userBudgetFound = await budgetRepository.GetByUserId(authenticatedUserId);

    if(userBudgetFound.IsSuccess) {
      await budgetRepository.DeleteByUserId(authenticatedUserId);
    }
    
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

    //validator required
    await budgetRepository.Create(newBudget);

    return Results.Ok();

  }

}