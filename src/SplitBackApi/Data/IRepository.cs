using MongoDB.Bson;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public interface IRepository {
  
  //Group
  // Task CreateGroup(Group group);
  
  // object GetGroupById(string id);
  
  //User
  Task<bool> UserExistsWithEmail(string email);
  
  Task AddUser(User user);
  
  Task<User> GetUserById(string userId);
  
  Task<User> GetUserByEmail(string email);
  
  //Session
  Task AddSession(Session session);
  
  Task<Session> GetSessionByRefreshToken(string refreshToken);
  
  Task<Session> GetSessionByUnique(string unique);
}