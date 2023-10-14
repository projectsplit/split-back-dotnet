using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.GroupRepository;

public interface IGroupRepository {

  Task Create(Group group);
  Task<Result<Group>> GetById(string groupId);
  Task<Result<List<Group>>> GetPaginatedGroupsByUserId(string userId, int limit, DateTime lastDateTime);
  Task<List<Group>> GetGroupsByUserId(string userId);
  Task<Result> Update(Group group);
  Task<Result> DeleteById(string groupId);

}