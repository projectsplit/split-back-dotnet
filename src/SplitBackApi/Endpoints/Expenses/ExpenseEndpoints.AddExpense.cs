using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  
  private static async Task<IResult> AddExpense(
    IRepository repo,
    HttpRequest request,
    NewExpenseDto newExpenseDto,
    IMapper mapper) {
    
    var groupId = new ObjectId(newExpenseDto.GroupId);

    var expenseValidator = new ExpenseValidator();
    var validationResult = expenseValidator.Validate(newExpenseDto);
    
    if(validationResult.Errors.Count > 0) {
      
      return Results.Ok(validationResult.Errors.Select(x => new {
        Message = x.ErrorMessage,
        Field = x.PropertyName
      }));
    }

    // ExpenseSetUp.AllocateAmountEqually(newExpenseDto);

    var newExpense = mapper.Map<Expense>(newExpenseDto);
    newExpense.CreationTime = DateTime.Now;

    var addNewExpenseResult = await repo.CreateExpense(newExpense, groupId);
    if(addNewExpenseResult.IsFailure) return Results.BadRequest(addNewExpenseResult.Error);
    
    var getGroupResult = await repo.GetGroupById(groupId);
    if(getGroupResult.IsFailure) return Results.BadRequest(getGroupResult.Error);
    var group = getGroupResult.Value;

    return Results.Ok(group.PendingTransactions());
  }
}