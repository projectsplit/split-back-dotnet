using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.GoogleUserRepository;

public interface IGoogleUserRepository {

  Task Create(GoogleUser user);
  Task<Result<GoogleUser>> GetById(string userId);
  Task<Result<GoogleUser>> GetBySub(string sub);
  Task<Result<List<GoogleUser>>> GetByIds(List<string> userIds);
  Task<Result<GoogleUser>> GetByEmail(string email);
  Task<Result> Update(GoogleUser editedUser);
  Task<Result> DeleteById(string userId);
}