using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.SessionRepository;

public interface ISessionRepository {
  
  Task Create(Session session);
  Task<Result<Session>> GetByRefreshToken(string refreshToken);
  Task<Result<Session>> GetByUnique(string unique);
  Task<List<Session>> GetLatest(int limit, DateTime last);
}