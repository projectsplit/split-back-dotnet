using SplitBackApi.Domain;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface IInvitationRepository {

  Task Create(Invitation invitation);
  Task<Result<Invitation>> GetById(string invitationId);
  Task<ICollection<Invitation>> GetByInviterId(string inviterId);
  Task<Result<Invitation>> GetByCode(string code);
  Task<Result> Update(Invitation editedInvitation);
  Task<Result> DeleteById(string invitationId);
}