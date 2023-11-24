using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.TransferRepository;

public interface ITransferRepository {

  Task Create(Transfer transfer);
    
  Task<Result<Transfer>> GetById(string transferId);
    
  Task<List<Transfer>> GetByGroupId(string groupId);
    
  Task<List<Transfer>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize);
    
  Task<Result> Update(Transfer editedTransfer);
    
  Task<Result> DeleteById(string transferId);
    
 Task<List<Transfer>> GetLatestByGroupsIdsMembersIdsAndStartDate(Dictionary<string, string> groupIdToMemberIdMap, DateTime startDate);
}