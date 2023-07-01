using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.GoogleUserRepository;

public interface IGoogleUserRepository {

  Task Create(GoogleUser user);
  Task<Maybe<GoogleUser>> GetById(string userId);
  Task<Maybe<GoogleUser>> GetBySub(string sub);
  Task<List<GoogleUser>> GetByIds(List<string> userIds);
  Task<Maybe<GoogleUser>> GetByEmail(string email);
  Task<Result> Update(GoogleUser editedUser);
  Task<Result> DeleteById(string userId);
}