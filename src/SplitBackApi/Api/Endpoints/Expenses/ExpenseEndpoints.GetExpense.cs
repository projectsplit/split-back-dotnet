using SplitBackApi.Data.Repositories.ExpenseRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using Microsoft.AspNetCore.Mvc;

namespace SplitBackApi.Api.Endpoints.Expenses;

public static partial class ExpenseEndpoints
{
  private static async Task<IResult> GetExpense(
    [FromQuery(Name = "expenseId")] string expenseId,
    IExpenseRepository expenseRepository,
    IGroupRepository groupRepository,
    IUserRepository userRepository)
  {
    var expenseMaybe = await expenseRepository.GetById(expenseId);
    if (expenseMaybe.HasNoValue) return Results.BadRequest("Expense not found");
    var expense = expenseMaybe.Value;

    var groupMaybe = await groupRepository.GetById(expense.GroupId);
    if (groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var response = new
    {
      expense.Id,
      expense.GroupId,
      expense.Amount,
      expense.Currency,
      expense.Description,
      expense.Labels,
      expense.Participants,
      expense.CreationTime,
      expense.ExpenseTime,
      expense.LastUpdateTime,
      expense.Payers,
    };

    return Results.Ok(response);
  }
}