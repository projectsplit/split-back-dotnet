using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Session> GetSessionByRefreshToken(string refreshToken) {
    
    return await _sessionCollection.Find(session => session.RefreshToken == refreshToken).SingleOrDefaultAsync();
  }
}