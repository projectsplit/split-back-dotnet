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
    
    var expenseMaybe = await expenseRepository.GetById(request.ExpenseId);
    if(expenseMaybe.HasNoValue) return Results.BadRequest("Expense not found");
    var expense = expenseMaybe.Value;

    var groupMaybe = await groupRepository.GetById(expense.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

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