using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> RemoveExpense(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    RemoveExpenseRequest request
  ) {
    
    var expenseResult = await expenseRepository.GetById(request.ExpenseId);
    if(expenseResult.IsFailure) return Results.BadRequest(expenseResult.Error);
    var expense = expenseResult.Value;

    var groupResult = await groupRepository.GetById(expense.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {expense.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    // ensure user can do this

    await expenseRepository.DeleteById(request.ExpenseId);
    var expenses = await expenseRepository.GetByGroupIdPerPage(request.GroupId, 1, 20);

    return Results.Ok(expenses);
  }
}