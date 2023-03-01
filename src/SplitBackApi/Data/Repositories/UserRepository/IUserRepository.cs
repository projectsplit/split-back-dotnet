using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.UserRepository;

public interface IUserRepository {

  Task Create(User user);
  Task<Result<User>> GetById(string userId);
  Task<Result<User>> GetByEmail(string email);
  Task<Result> Update(User editedUser);
  Task<Result> DeleteById(string userId);
}