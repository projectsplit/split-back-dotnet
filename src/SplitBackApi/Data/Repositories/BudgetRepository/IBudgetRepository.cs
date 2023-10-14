using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data.Repositories.BudgetRepository;

public interface IBudgetRepository
{
    Task<Result> Create(Budget budget, CancellationToken ct);

    Task<Result> DeleteByUserId(string userId);

    Task<Maybe<Budget>> GetByUserId(string userId);
    
    // Task<Result<Expense>> GetExpensesByBudgetType(BudgetType BudgetType, string day);
}