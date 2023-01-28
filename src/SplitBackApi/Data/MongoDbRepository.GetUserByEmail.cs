using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<User> GetUserByEmail(string email) {
    
    var filter = Builders<User>.Filter.Eq("Email", email);
    
    return await _userCollection.Find(filter).SingleOrDefaultAsync();
  }
}