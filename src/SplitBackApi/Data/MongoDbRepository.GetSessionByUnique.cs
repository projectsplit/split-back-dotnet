using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Session> GetSessionByUnique(string unique) {
    
    return await _sessionCollection.Find(session => session.Unique == unique).SingleOrDefaultAsync();
  }
}