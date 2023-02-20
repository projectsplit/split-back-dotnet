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
  Task<Result> RestoreGuestToGroup(string groupId, string userId);


  // Group
  Task<Result> CreateGroup(Group group);
  Task<Result<Group>> GetGroupById(string groupId);
  Task<Result> AddUserMemberToGroup(string groupId, string userId, List<string> roleIds);
  Task<Result> CreateRole(string groupId, string roleName, Permissions rolePermissions);
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
  Task AddTransferToHistory(IClientSessionHandle session, Group oldGroup, string Id, FilterDefinition<Group>? filter);

  // Invitation
  Task CreateUserInvitation(string inviterId, string groupId);
  Task<Invitation> GetInvitationByInviter(string userId, string groupId);
  Task<Result<Invitation>> GetInvitationByCode(string Code);
  Task<Result> DeleteUserInvitation(string userId, string groupId);
  Task<Result> RegenerateUserInvitation(string inviterId, string groupId);
  Task<Result> ReplaceGuestMemberWithUserMember(string groupId, UserMember userMember, string guestId);


  //Guest Invitation
  Task<Invitation> GetGuestInvitationByInviterIdAndGuestId(string inviterId, string groupId, string guestId);
  Task CreateGuestInvitation(string inviterId, string groupId, string guestId);
  Task<Result<Invitation>> GetGuestInvitationByCode(string Code);
  Task<Result> DeleteGuestInvitation(string inviterId, string groupId, string guestId);
  Task<Result> RegenerateGuestInvitation(string inviterId, string groupId, string guestId);


  // Comment
  Task<Result> AddComment(Comment newComment, string expenseId, string groupId);
}