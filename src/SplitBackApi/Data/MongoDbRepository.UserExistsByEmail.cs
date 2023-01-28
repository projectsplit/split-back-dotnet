using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<bool> UserExistsByEmail(string email) {
    
    var filter = Builders<User>.Filter.Eq("Email", email);
    
    var userCount = await _userCollection.CountDocumentsAsync(filter);
    
    return userCount > 0;
  }
}