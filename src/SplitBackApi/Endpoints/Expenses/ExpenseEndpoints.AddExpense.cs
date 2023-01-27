using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;
public static partial class ExpenseEndpoints {
  private static async Task<IResult> AddExpense(IRepository repo, HttpRequest request, NewExpenseDto newExpenseDto, IMapper mapper) {
    var groupId = new ObjectId(newExpenseDto.GroupId);

    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(newExpenseDto);
    if(validationResult.Errors.Count > 0) {
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    ExpenseSetUp.AllocateAmountEqually(newExpenseDto);

    var newExpense = mapper.Map<Expense>(newExpenseDto);
    newExpense.CreationTime = DateTime.Now;

    var addExpenseRes = await repo.AddNewExpense(newExpense, groupId);
    if(addExpenseRes.IsFailure) return Results.BadRequest(addExpenseRes.Error);
    
    var getGroupRes = await repo.GetGroupById(groupId);
    if(getGroupRes.IsFailure) return Results.BadRequest(getGroupRes.Error);

    return Results.Ok(getGroupRes.Value.PendingTransactions());
  }
}