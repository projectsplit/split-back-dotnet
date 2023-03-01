using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.InvitationRepository;

public interface IInvitationRepository {

  Task Create(Invitation invitation);
  Task<Result<Invitation>> GetById(string invitationId);
  Task<ICollection<Invitation>> GetByInviterId(string inviterId);
  Task<Result<Invitation>> GetByCode(string code);
  Task<Result> Update(Invitation editedInvitation);
  Task<Result> DeleteById(string invitationId);
}