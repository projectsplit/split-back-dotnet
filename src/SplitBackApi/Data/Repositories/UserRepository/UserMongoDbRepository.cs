using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.UserRepository;

public class UserMongoDbRepository : IUserRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<User> _userCollection;

  public UserMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Database.Collections.Users);
  }

  public async Task Create(User user) {

    await _userCollection.InsertOneAsync(user);
  }

  public async Task<Result> DeleteById(string userId) {

    var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

    var deleteResult = await _userCollection.DeleteOneAsync(filter);

    if(deleteResult.IsAcknowledged is false) return Result.Failure($"Failed to delete user with id {userId}");

    return Result.Success();
  }

  public async Task<Result<User>> GetByEmail(string email) {

    var filter = Builders<User>.Filter.Eq(u => u.Email, email);

    var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
    if(user is null) return Result.Failure<User>($"User with email {email} has not been found");

    return user;
  }

  public async Task<Result<User>> GetById(string userId) {

    var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

    var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
    if(user is null) return Result.Failure<User>($"User with id {userId} has not been found");

    return user;
  }

  public async Task<Result<List<User>>> GetByIds(List<string> userIds) {
    
    var filter = Builders<User>.Filter.In(u => u.Id, userIds);
    var users = await _userCollection.Find(filter).ToListAsync();

    if(users is null) return Result.Failure<List<User>>("None of the provided user ids were found.");
  
    return users;
  }

  public async Task<Result> Update(User editedUser) {

    var filter = Builders<User>.Filter.Eq(u => u.Id, editedUser.Id);

    var replaceOneResult = await _userCollection.ReplaceOneAsync(filter, editedUser);

    if(replaceOneResult.IsAcknowledged is false) return Result.Failure($"Failed to update user with id {editedUser.Id}");

    if(replaceOneResult.MatchedCount == 0) return Result.Failure($"User with id {editedUser.Id} has not been found");

    return Result.Success();
  }
}