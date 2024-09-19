using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data.Repositories.GroupFiltersRepository;

public interface IGroupFiltersRepository
{
    Task<Result> Create(GroupFilter groupFilter);

    Task<Result> Update(GroupFilter groupFilter);

    Task<Result<GroupFilter>> GetByGroupId(string groupId);
}