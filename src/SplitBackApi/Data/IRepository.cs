using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Endpoints.Requests;
using MongoDB.Driver;
using CSharpFunctionalExtensions;
namespace SplitBackApi.Data;

public interface IRepository {

  Task<bool> UserExistsWithEmail(string email);

  Task AddUser(User user);

  // Task<User> GetUserById(string userId);

  Task<User> GetUserByEmail(string email);
  //Session
  Task AddSession(Session session);

  Task<Session> GetSessionByRefreshToken(string refreshToken);

  Task<Session> GetSessionByUnique(string unique);

  Task<Result> CreateGroup(Group group);

  Task<Result> AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId);

  Task AddExpenseToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);

  Task AddTransferToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);

  Task AddLabel(Label label);

  Task<Result> AddNewExpense(Expense newExpense, ObjectId groupId);

  Task<Result> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId);

  Task<Result> RemoveExpense(string groupId, string expenseId);

  Task<Result> RestoreExpense(string groupId, string expenseId);

  Task<Result> AddNewTransfer(Transfer newTransfer, ObjectId groupId);

  Task<Result> EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId);

  Task<Result> RemoveTransfer(string groupId, string transferId);

  Task<Result> RestoreTransfer(string groupId, string transferId);

  Task<Result<Group>> GetGroupById(ObjectId groupId);

  Task<bool> EmailExists(string Email);

  Task<Result<User>> GetUserById(ObjectId userId);

  Task AddUserToGroup(ObjectId groupId, ObjectId UserId);

  Task CreateInvitation(ObjectId inviterId, ObjectId groupId);

  Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId);

  Task<Result<Invitation>> GetInvitationByCode(string Code);

  Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId);

  Task<Result<Group>> CheckIfUserInGroupMembers(ObjectId userId, ObjectId groupId);

  Task<Result<Group>> CheckAndAddUserInGroupMembers(ObjectId userId, ObjectId groupId);

  Task<Result<User>> CheckIfGroupInUser(ObjectId userId, ObjectId groupId);

  Task<Result<User>> CheckAndAddGroupInUser(ObjectId userId, ObjectId groupId);

  Task<Result> CreateRole(ObjectId groupId, string roleName, Role newRole);

  Task<Result> EditRole(ObjectId roleId, ObjectId groupId, string roleName, Role newRole);

  Task<Result> AddRoleToUser(ObjectId groupId, ObjectId userId, ObjectId roleId);

  Task<Result> RemoveRoleFromUser(ObjectId groupId, ObjectId userId, ObjectId roleId);
}