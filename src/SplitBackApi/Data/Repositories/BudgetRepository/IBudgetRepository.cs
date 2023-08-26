using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data.Repositories.BudgetRepository;

public interface IBudgetRepository
{
  Task Create(Budget budget);
  Task<Result> DeleteByUserId(string UserId);
  Task<Result<Budget>> GetByUserId(string UserId);
  // Task<Result<Expense>> GetExpensesByBudgetType(BudgetType BudgetType, string day);
}