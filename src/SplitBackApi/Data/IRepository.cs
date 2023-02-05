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
  Task<Result<User>> GetUserById(string userId);
  Task<User> GetUserByEmail(string email);
  Task<Result> AddGuestToGroup(string groupId, string email, string nickname);
  Task<Result> RemoveGuestFromGroup(string groupId, string userId);
  Task<Result> RestoreGuestToGroup(string groupId, string userId);
  // Task<bool> EmailExists(string Email);
  Task<Result<User>> GetUserIfGroupNotExistsInUserGroups(string userId, string groupId);
  Task<Result<User>> AddGroupInUser(string userId, string groupId);

  // Group
  Task<Result> CreateGroup(Group group);
  Task<Result<Group>> GetGroupById(string groupId);
  Task AddUserToGroup(IClientSessionHandle session, string groupID, string userID, ICollection<string> roleIDs);
  
  
  // AddUserToGroup
  // RemoveUserFromGroup
  // AddGuestToGroup
  // RemoveGuestFromGroup
  
  // CreateMember
  
  
  Task<Result<Group>> GetGroupIfUserIsNotMember(string userId, string groupId);
  Task<Result<Group>> AddUserInGroupMembers(string userId, string groupId);
  Task<Result> CreateRole(string groupId, string roleName, Role newRole);
  Task<Result> EditRole(string roleId, string groupId, string roleName, Role newRole);
  Task<Result> AddRoleToUser(string groupId, string userId, string roleId);
  Task<Result> RemoveRoleFromUser(string groupId, string userId, string roleId);

  // Expense
  Task<Result> CreateExpense(Expense newExpense, string groupId);
  Task<Result> EditExpense(Expense newExpense, string groupId, string expenseId);
  Task<Result> RemoveExpense(string groupId, string expenseId);
  Task AddExpenseToHistory(IClientSessionHandle session, Group oldGroup, string Id, FilterDefinition<Group>? filter);
  Task<Result> RestoreExpense(string groupId, string expenseId);

  // Trasfer
  Task<Result> CreateTransfer(Transfer newTransfer, string groupId);
  Task<Result> EditTransfer(Transfer newTransfer, string groupId, string transferId);
  Task<Result> RemoveTransfer(string groupId, string transferId);
  Task<Result> RestoreTransfer(string groupId, string transferId);
  Task AddTransferToHistory(IClientSessionHandle session,Group oldGroup, string Id, FilterDefinition<Group>? filter);

  // Invitaion
  Task CreateInvitation(string inviterId, string groupId);
  Task<Invitation> GetInvitationByInviter(string userId, string groupId);
  Task<Result<Invitation>> GetInvitationByCode(string Code);
  Task<DeleteResult> DeleteInvitation(string userId, string groupId);

  // Comment
  Task<Result> AddComment(Comment newComment, string expenseId, string groupId);
}