using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.TransferRepository;

public interface ITransferRepository
{

  Task Create(Transfer transfer);

  Task<Result<Transfer>> GetById(string transferId);

  Task<List<Transfer>> GetByGroupId(string groupId);

  Task<List<Transfer>> GetByGroupIds(List<string> groupIds);

  // Task<List<Transfer>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize);

  Task<Result> Update(Transfer editedTransfer);

  Task<Result> DeleteById(string transferId);

  Task<Result<List<Transfer>>> GetPaginatedTransfersByGroupId(string groupId, int limit, DateTime last);
  Task<List<Transfer>> GetLatestByGroupsIdsMembersIdsStartDateEndDate(Dictionary<string, string> groupIdToMemberIdMap, DateTime startDate, DateTime endDate);
}