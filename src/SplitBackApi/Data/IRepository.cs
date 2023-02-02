using MongoDB.Bson;
using SplitBackApi.Domain;
using MongoDB.Driver;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface IRepository {

  // Session
  Task CreateSession(Session session);
  Task<Session> GetSessionByRefreshToken(string refreshToken);
  Task<Session> GetSessionByUnique(string unique);

  // User
  Task<bool> UserExistsByEmail(string email);
  Task CreateUser(User user);
  Task<Result<User>> GetUserById(ObjectId userId);
  Task<User> GetUserByEmail(string email);
  Task<Result> AddGuestToGroup(ObjectId groupId, string email, string nickname);
  Task<Result> RemoveGuestFromGroup(ObjectId groupId, ObjectId userId);
  Task<Result> RestoreGuestToGroup(ObjectId groupId, ObjectId userId);
  // Task<bool> EmailExists(string Email);
  Task<Result<User>> GetUserIfGroupNotExistsInUserGroups(ObjectId userId, ObjectId groupId);
  Task<Result<User>> AddGroupInUserOrFail(ObjectId userId, ObjectId groupId);

  // Group
  Task<Result> CreateGroup(Group group);
  Task<Result<Group>> GetGroupById(ObjectId groupId);
  Task AddUserToGroup(ObjectId groupId, ObjectId UserId);
  Task<Result<Group>> GetGroupIfUserIsNotMember(ObjectId userId, ObjectId groupId);
  Task<Result<Group>> AddUserInGroupMembersOrFail(ObjectId userId, ObjectId groupId);
  Task<Result> CreateRole(ObjectId groupId, string roleName, Role newRole);
  Task<Result> EditRole(ObjectId roleId, ObjectId groupId, string roleName, Role newRole);
  Task<Result> AddRoleToUser(ObjectId groupId, ObjectId userId, ObjectId roleId);
  Task<Result> RemoveRoleFromUser(ObjectId groupId, ObjectId userId, ObjectId roleId);

  // Expense
  Task<Result> CreateExpense(Expense newExpense, ObjectId groupId);
  Task<Result> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId);
  Task<Result> RemoveExpense(string groupId, string expenseId);
  Task AddExpenseToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);
  Task<Result> RestoreExpense(string groupId, string expenseId);

  // Trasfer
  Task<Result> CreateTransfer(Transfer newTransfer, ObjectId groupId);
  Task<Result> EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId);
  Task<Result> RemoveTransfer(string groupId, string transferId);
  Task<Result> RestoreTransfer(string groupId, string transferId);
  Task AddTransferToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter);

  // Invitaion
  Task CreateInvitation(ObjectId inviterId, ObjectId groupId);
  Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId);
  Task<Result<Invitation>> GetInvitationByCode(string Code);
  Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId);

  // Comment
  Task<Result> AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId);
}