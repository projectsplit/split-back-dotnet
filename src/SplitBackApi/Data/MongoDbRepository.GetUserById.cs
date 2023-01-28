using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<User>> GetUserById(ObjectId userId) {
    
    var user = await _userCollection.Find(user => user.Id == userId).SingleOrDefaultAsync();
    if(user is null) return Result.Failure<User>($"user {userId} not found");
    
    return user;
  }
}