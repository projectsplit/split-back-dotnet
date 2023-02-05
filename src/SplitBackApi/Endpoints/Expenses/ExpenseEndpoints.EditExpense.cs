using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using MongoDB.Bson;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  
  private static async Task<IResult> EditExpense(
    IRepository repo,
    EditExpenseDto editExpenseDto,
    IMapper mapper) {

    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(editExpenseDto);

    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    var newExpense = mapper.Map<Expense>(editExpenseDto);

    var editExpenseResult = await repo.EditExpense(newExpense, editExpenseDto.GroupId, editExpenseDto.ExpenseId);
    if(editExpenseResult.IsFailure) return Results.BadRequest(editExpenseResult.Error);

    var getGroupResult = await repo.GetGroupById(editExpenseDto.GroupId);
    if(getGroupResult.IsFailure) return Results.BadRequest(getGroupResult.Error);
    var group = getGroupResult.Value;

    return Results.Ok(group.PendingTransactions());
  }
}