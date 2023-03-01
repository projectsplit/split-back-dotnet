using SplitBackApi.Domain;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public interface ITransferRepository {

  Task Create(Transfer transfer);
  Task<Result<Transfer>> GetById(string transferId);
  Task<List<Transfer>> GetByGroupId(string groupId);
  Task<Result> Update(Transfer editedTransfer);
  Task<Result> DeleteById(string transferId);
}