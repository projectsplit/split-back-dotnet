using CSharpFunctionalExtensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public interface ISessionRepository {
  
  Task Create(Session session);
  Task<Result<Session>> GetByRefreshToken(string refreshToken);
  Task<Result<Session>> GetByUnique(string unique);
}