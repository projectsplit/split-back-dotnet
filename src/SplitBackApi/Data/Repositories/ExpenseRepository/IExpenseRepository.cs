using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExpenseRepository;

public interface IExpenseRepository
{
    Task Create(Expense expense);
    
    Task<Result<Expense>> GetById(string expenseId);
    
    Task<List<Expense>> GetByGroupId(string groupId);
    
    Task<List<Expense>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize);
    
    Task<Result> Update(Expense editedExpense);
    
    Task<Result> DeleteById(string expenseId);
    
    Task<List<Expense>> GetLatest(string groupId, int limit, DateTime last);
    
    Task<List<Expense>> GetLatestByGroupIdMemberId(string groupId, string memberId, DateTime startDate);
}