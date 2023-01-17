using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Helper;
using MongoDB.Bson;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> EditExpense(IRepository repo, EditExpenseDto editExpenseDto, IMapper mapper) {

    var groupId = ObjectId.Parse(editExpenseDto.GroupId);
    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(editExpenseDto);

    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    ExpenseSetUp.AllocateAmountEqually(editExpenseDto);

   var newExpense = mapper.Map<Expense>(editExpenseDto);
   var expenseId = ObjectId.Parse(editExpenseDto.ExpenseId);

   await repo.EditExpense(newExpense, groupId, expenseId);
   
    var getGroupResult = await repo.GetGroupById(groupId);

    return getGroupResult.Match(group => {
      return Results.Ok(group.PendingTransactions());

    }, e => {

      return Results.BadRequest(e.Message);
    });
  }
}