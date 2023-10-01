using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> EditExpense(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    ExpenseValidator expenseValidator,
    EditExpenseRequest request
  ) {

    var expenseMaybe = await expenseRepository.GetById(request.ExpenseId);
    if(expenseMaybe.HasNoValue) return Results.BadRequest("Expense not found");
    var expense = expenseMaybe.Value;

    var groupMaybe = await groupRepository.GetById(expense.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var groupMemberIds = group.Members.Select(m => m.MemberId).ToList();
    
    var requestMemberIds = request.Payers
    .Select(p => p.MemberId)
    .Union(request.Participants
    .Select(p => p.MemberId)).ToList();

    var memberIdsAreValid = requestMemberIds.Intersect(groupMemberIds).Count() == requestMemberIds.Count();
    if(memberIdsAreValid is false) return Results.BadRequest("Invalid member Id(s)");

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {expense.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    var oldExpenseMaybe = await expenseRepository.GetById(request.ExpenseId);
    if(oldExpenseMaybe.HasNoValue) return Results.BadRequest("Expense not found");
    var oldExpense = oldExpenseMaybe.Value;

    var editedExpense = new Expense {
      Id = oldExpense.Id,
      Description = request.Description,
      Amount = request.Amount,
      CreationTime = oldExpense.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      Currency = request.Currency,
      ExpenseTime = request.ExpenseTime,
      GroupId = expense.GroupId,
      Labels = request.Labels,
      Participants = request.Participants.Select(p => new Participant {
        MemberId = p.MemberId,
        ParticipationAmount = p.ParticipationAmount
      }).ToList(),
      Payers = request.Payers.Select(p => new Payer {
        MemberId = p.MemberId,
        PaymentAmount = p.PaymentAmount
      }).ToList()
    };

    var validationResult = expenseValidator.Validate(editedExpense);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var updateResult = await expenseRepository.Update(editedExpense);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update expense");

    return Results.Ok();
  }
}