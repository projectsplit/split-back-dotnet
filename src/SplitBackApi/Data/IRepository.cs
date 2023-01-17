using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Endpoints.Requests;
using LanguageExt.Common;
using MongoDB.Driver;
using LanguageExt;

namespace SplitBackApi.Data;

public interface IRepository {
  
  Task<bool> UserExistsWithEmail(string email);
  
  Task AddUser(User user);
  
  Task<User> GetUserById(string userId);
  
  Task<User> GetUserByEmail(string email);
  //Session
  Task AddSession(Session session);
  
  Task<Session> GetSessionByRefreshToken(string refreshToken);
  
  Task<Session> GetSessionByUnique(string unique);

  Task CreateGroup(Group group);

  Task AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId);

  Task AddExpenseToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);

  Task AddTransferToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);

  Task AddLabel(Label label);

  Task AddNewExpense(Expense newExpense, ObjectId groupId);

  Task<Result<Unit>> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId);

  Task RemoveExpense(string groupId, string expenseId);

  Task RestoreExpense(string groupId, string expenseId);

  Task AddNewTransfer(Transfer newTransfer, ObjectId groupId);

  Task EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId);

  Task RemoveTransfer(string groupId, string transferId);

  Task RestoreTransfer(string groupId, string transferId);

  Task<Result<Group>> GetGroupById(ObjectId groupId);

  Task<bool> EmailExists(string Email);

  Task<User> GetUserById(ObjectId userId);

  Task AddUserToGroup(ObjectId groupId, ObjectId UserId);

  Task CreateInvitation(ObjectId inviterId, ObjectId groupId);

  Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId);

  Task<Invitation> GetInvitationByCode(string Code);

  Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId);

  Task<Group> CheckIfUserInGroupMembers(ObjectId userId, ObjectId groupId);

  Task<Group> CheckAndAddUserInGroupMembers(ObjectId userId, ObjectId groupId);

  Task<User> CheckIfGroupInUser(ObjectId userId, ObjectId groupId);

  Task<User> CheckAndAddGroupInUser(ObjectId userId, ObjectId groupId);
}