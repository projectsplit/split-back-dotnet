using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task AddTransferToHistory(IClientSessionHandle session, Group oldGroup, string OperationId, FilterDefinition<Group>? filter) {

    var oldTransfer = oldGroup.Transfers.First(t => t.Id == OperationId);

    var snapShot = _mapper.Map<TransferSnapshot>(oldTransfer);

    var update = Builders<Group>.Update.Push("Transfers.$.History", snapShot);

    await _groupCollection.FindOneAndUpdateAsync(session, filter, update);
  }
}