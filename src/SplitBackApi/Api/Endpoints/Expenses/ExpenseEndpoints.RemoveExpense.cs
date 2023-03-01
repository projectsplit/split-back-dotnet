using SplitBackApi.Api.Endpoints.Expenses.Requests;
using SplitBackApi.Data.Repositories.ExpenseRepository;

namespace SplitBackApi.Api.Endpoints.Expenses;

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