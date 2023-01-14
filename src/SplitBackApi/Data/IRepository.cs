using MongoDB.Bson;
using SplitBackApi.Domain;
using SplitBackApi.Endpoints.Requests;
using LanguageExt.Common;
using MongoDB.Driver;

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

  Task AddComment(NewCommentDto newComment, ObjectId userId);

  Task AddToHistory(Group oldGroup, ObjectId Id, FilterDefinition<Group>? filter, bool isExpense);

  Task AddLabel(Label label);

  Task AddNewExpense(NewExpenseDto newExpenseDto);

  Task EditExpense(EditExpenseDto editExpenseDto);

  Task RemoveExpense(RemoveRestoreExpenseDto removeRestoreExpenseDto);

  Task RestoreExpense(RemoveRestoreExpenseDto removeRestoreExpenseDto);

  Task AddNewTransfer(NewTransferDto newTransferDto);

  Task EditTransfer(EditTransferDto editTransferDto);

  Task RemoveTransfer(RemoveRestoreTransferDto removeRestoreTransferDto);

  Task RestoreTransfer(RemoveRestoreTransferDto removeRestoreTransferDto);

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