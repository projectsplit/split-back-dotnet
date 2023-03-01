using SplitBackApi.Domain;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface IExpenseRepository {

  Task Create(Expense expense);
  Task<Result<Expense>> GetById(string expenseId);
  Task<List<Expense>> GetByGroupId(string groupId);
  Task<Result> Update(Expense editedExpense);
  Task<Result> DeleteById(string expenseId);
}