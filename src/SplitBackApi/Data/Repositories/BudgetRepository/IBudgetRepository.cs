using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using MongoDB.Driver;

namespace SplitBackApi.Data.Repositories.BudgetRepository;

public interface IBudgetRepository
{
    Task<Result> Create(Budget budget,string userId, CancellationToken ct);

    Task<Result> DeleteByUserId(string userId);

    Task<Maybe<Budget>> GetByUserId(string userId);
    
}