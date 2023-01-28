using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task CreateUser(User user) {
    
    await _userCollection.InsertOneAsync(user);
  }
}