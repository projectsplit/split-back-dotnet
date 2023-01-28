using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task CreateSession(Session session) {
    
    await _sessionCollection.InsertOneAsync(session);
  }
}