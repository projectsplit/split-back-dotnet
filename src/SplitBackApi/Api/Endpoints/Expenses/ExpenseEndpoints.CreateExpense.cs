using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> CreateExpense(
    IGroupRepository groupRepository,
    IExpenseRepository expenseRepository,
    CreateExpenseRequest request
  ) {

    // ensure user has permissions ??

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

    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(newExpense);
    if(!validationResult.IsValid) return Results.BadRequest(validationResult.Errors.Select(e =>
      new {
        Field = e.PropertyName,
        ErrorMessage = e.ErrorMessage
      }
    ));

    await expenseRepository.Create(newExpense);

    return Results.Ok();
  }
}