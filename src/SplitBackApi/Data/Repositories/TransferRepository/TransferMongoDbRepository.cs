using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Api.Helper;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.TransferRepository;

public class TransferMongoDbRepository : ITransferRepository
{

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Transfer> _transferCollection;
  private readonly IMongoCollection<PastTransfer> _pastTransferCollection;

  public TransferMongoDbRepository(
    IOptions<AppSettings> appSettings
  )
  {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _transferCollection = mongoDatabase.GetCollection<Transfer>(dbSettings.Database.Collections.Transfers);
    _pastTransferCollection = mongoDatabase.GetCollection<PastTransfer>(dbSettings.Database.Collections.PastTransfers);
  }

  public async Task Create(Transfer transfer)
  {

    await _transferCollection.InsertOneAsync(transfer);
  }

  public async Task<Result> DeleteById(string transferId)
  {

    var findTransferFilter = Builders<Transfer>.Filter.Eq(t => t.Id, transferId);

    var transferFound = await _transferCollection.Find<Transfer>(findTransferFilter).FirstOrDefaultAsync();
    if (transferFound is null) return Result.Failure($"Transfer with id {transferId} has not been found to be deleted");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try
    {

      await _transferCollection.DeleteOneAsync(session, findTransferFilter);

      await _pastTransferCollection.InsertOneAsync(session, transferFound.ToPastTransfer());

      await session.CommitTransactionAsync();

    }
    catch (MongoException e)
    {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<Result<Transfer>> GetById(string transferId)
  {

    var findTransferFilter = Builders<Transfer>.Filter.Eq(t => t.Id, transferId);
    var transfer = await _transferCollection.Find(findTransferFilter).FirstOrDefaultAsync();
    if (transfer is null) return Result.Failure<Transfer>($"Transfer with id {transferId} has not been found");

    return transfer;
  }

  public async Task<List<Transfer>> GetByGroupId(string groupId)
  {

    var filter = Builders<Transfer>.Filter.Eq(t => t.GroupId, groupId);
    return await _transferCollection.Find(filter).ToListAsync();
  }

  public async Task<List<Transfer>> GetByGroupIds(List<string> groupIds)
  {
    var filter = Builders<Transfer>.Filter.In(t => t.GroupId, groupIds);
    var transfers = await _transferCollection.Find(filter).ToListAsync();
    return transfers;
  }

  public async Task<List<Transfer>> GetByGroupIdPerPage(string groupId, int pageNumber, int pageSize)
  {

    var filter = Builders<Transfer>.Filter.Eq(e => e.GroupId, groupId);
    var query = _transferCollection.Find(filter);

    int skipCount = (pageNumber - 1) * pageSize;
    query = query.Limit(pageSize);
    query = query.Skip(skipCount);

    return await query.ToListAsync();
  }

  public async Task<Result> Update(Transfer editedTransfer)
  {

    var findTransferFilter = Builders<Transfer>.Filter.Eq(t => t.Id, editedTransfer.Id);

    var currentTransfer = await _transferCollection.Find(findTransferFilter).FirstOrDefaultAsync();
    if (currentTransfer is null) return Result.Failure($"Transfer with id {editedTransfer.Id} has not been found");

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try
    {
      await _transferCollection.ReplaceOneAsync(session, findTransferFilter, editedTransfer);

      await _pastTransferCollection.InsertOneAsync(session, currentTransfer.ToPastTransfer());

      await session.CommitTransactionAsync();

    }
    catch (MongoException e)
    {

      await session.AbortTransactionAsync();

      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }

  public async Task<List<Transfer>> GetLatestByGroupsIdsMembersIdsStartDateEndDate(
      Dictionary<string, string> groupIdToMemberIdMap,
      DateTime startDate,DateTime endDate)
  {
    var groupIds = groupIdToMemberIdMap.Keys.ToList();

    var groupFilter = Builders<Transfer>.Filter.In(t => t.GroupId, groupIds);
    var creationTimeFilter =
        Builders<Transfer>.Filter.Gte(t => t.TransferTime, startDate) &
        Builders<Transfer>.Filter.Lte(t => t.TransferTime, endDate);

    var transfers = await _transferCollection.Find(groupFilter & creationTimeFilter).ToListAsync();

    var senderFilter = transfers
    .Where(t => groupIds.Contains(t.GroupId) && t.SenderId == groupIdToMemberIdMap[t.GroupId])
    .ToList();

    var receiverFilter = transfers
    .Where(t => groupIds.Contains(t.GroupId) && t.ReceiverId == groupIdToMemberIdMap[t.GroupId])
    .ToList();

    var filteredTransfers = senderFilter.Concat(receiverFilter).ToList();

    return filteredTransfers;
  }
}