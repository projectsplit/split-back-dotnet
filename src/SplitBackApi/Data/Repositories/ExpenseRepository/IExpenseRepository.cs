using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.ExpenseRepository;

public interface IExpenseRepository
{
    Task Create(Expense expense);

    Task<Result<Expense>> GetById(string expenseId);

    Task<List<Expense>> GetByGroupId(string groupId);
    
    Task<List<Expense>> GetByGroupIds(List<string> groupIds);

    Task<List<Expense>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize);

    Task<Result> Update(Expense editedExpense);

    Task<Result> DeleteById(string expenseId);

    Task<Result<List<Expense>>> GetPaginatedExpensesByGroupId(string groupId, int limit, DateTime last);

    Task<List<Expense>> GetLatestByGroupsIdsMembersIdsStartDateEndDate(Dictionary<string, string> groupIdToMemberIdMap, DateTime startDate, DateTime endDate);
    
}