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

  private static async Task<IResult> CreateExpense(
    IGroupRepository groupRepository,
    ClaimsPrincipal claimsPrincipal,
    IExpenseRepository expenseRepository,
    ExpenseValidator expenseValidator,
    CreateExpenseRequest request
  ) {

    // ensure user has permissions ??
    var groupMaybe = await groupRepository.GetById(request.GroupId);
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
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    var newExpensePayers = request.Payers
      .Select(p => new Payer {
        MemberId = p.MemberId,
        PaymentAmount = p.PaymentAmount
      })
      .ToList();

    var newExpenseParticipants = request.Participants
      .Select(p => new Participant {
        MemberId = p.MemberId,
        ParticipationAmount = p.ParticipationAmount
      })
      .ToList();

    var newExpense = new Expense {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      Amount = request.Amount,
      Currency = request.Currency,
      Description = request.Description,
      GroupId = request.GroupId,
      ExpenseTime = request.ExpenseTime,
      Labels = request.Labels,
      Payers = newExpensePayers,
      Participants = newExpenseParticipants
    };

    var validationResult = expenseValidator.Validate(newExpense);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    await expenseRepository.Create(newExpense);

    var expenses = await expenseRepository.GetByGroupIdPerPage(request.GroupId, 1, 20);
    return Results.Ok(expenses);
  }
}