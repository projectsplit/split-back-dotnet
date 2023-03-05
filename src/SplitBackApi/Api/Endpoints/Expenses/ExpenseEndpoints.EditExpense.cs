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
    EditExpenseRequest request
  ) {

    var expenseResult = await expenseRepository.GetById(request.ExpenseId);
    if(expenseResult.IsFailure) return Results.BadRequest(expenseResult.Error);
    var expense = expenseResult.Value;

    var groupResult = await groupRepository.GetById(expense.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

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

    var oldExpenseResult = await expenseRepository.GetById(request.ExpenseId);
    if(oldExpenseResult.IsFailure) return Results.BadRequest(oldExpenseResult.Error);
    var oldExpense = oldExpenseResult.Value;

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

    //TODO validate edited expense
    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(editedExpense);
    if(!validationResult.IsValid) return Results.BadRequest(validationResult.Errors.Select(e =>
      new {
        Field = e.PropertyName,
        ErrorMessage = e.ErrorMessage
      }
    ));

    var updateResult = await expenseRepository.Update(editedExpense);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update expense");

    return Results.Ok();
  }
}