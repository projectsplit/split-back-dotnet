using System.Security.Claims;
using SplitBackApi.Data.Repositories.ExpenseRepository;


// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetGroupExpenses(
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    HttpRequest request
  ) {
    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("group id query is missing");
    
    var expenses = await expenseRepository.GetByGroupId(groupId);

    return Results.Ok(expenses);
  }
}