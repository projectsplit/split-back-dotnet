using SplitBackApi.Domain;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface IGroupRepository {

  Task Create(Group group);
  Task<Result<Group>> GetById(string groupId);
  Task<Result> Update(Group group);
  Task<Result> DeleteById(string groupId);
}