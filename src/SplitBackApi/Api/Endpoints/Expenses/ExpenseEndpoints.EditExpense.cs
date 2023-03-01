using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> EditExpense(
    IExpenseRepository expenseRepository,
    EditExpenseRequest request
  ) {

    var oldExpenseResult = await expenseRepository.GetById(request.ExpenseId);
    if(oldExpenseResult.IsFailure) Results.BadRequest(oldExpenseResult.Error);
    var oldExpense = oldExpenseResult.Value;

    var editedExpense = new Expense {
      Id = oldExpense.Id,
      Description = request.Description,
      Amount = request.Amount,
      CreationTime = oldExpense.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      Currency = request.Currency,
      ExpenseTime = request.ExpenseTime,
      GroupId = request.GroupId,
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

    var updateResult = await expenseRepository.Update(editedExpense);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update expense");

    return Results.Ok();
  }
}