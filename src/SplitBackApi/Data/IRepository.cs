using MongoDB.Bson;
using SplitBackApi.Entities;

namespace SplitBackApi.Data;

public interface IRepository {
  
  //User
  Task<bool> UserExistsWithEmail(string email);
  
  Task AddUser(User user);
  
  Task<User> GetUserById<TId>(TId userId);
  
  Task<User> GetUserByEmail(string email);
  
  //Session
  Task AddSession(Session session);
  
  Task<Session> GetSessionByRefreshToken(string refreshToken);
  
  Task<Session> GetSessionByUnique(string unique);
}