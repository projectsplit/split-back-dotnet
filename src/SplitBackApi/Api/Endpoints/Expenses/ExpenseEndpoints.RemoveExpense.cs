using SplitBackApi.Data;
using SplitBackApi.Requests;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  
  private static async Task<IResult> RemoveExpense(
    IExpenseRepository expenseRepository,
    RemoveExpenseRequest request
  ) {    
    
    // ensure user can do this
    
    await expenseRepository.DeleteById(request.ExpenseId);
    
    return Results.Ok();
  }
}