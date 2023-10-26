using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.GoogleUserRepository;

public class GoogleUserMongoDbRepository : IGoogleUserRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<GoogleUser> _googleUserCollection;

  public GoogleUserMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _googleUserCollection = mongoDatabase.GetCollection<GoogleUser>(dbSettings.Database.Collections.GoogleUsers);
  }

  public async Task Create(GoogleUser user) {

    await _googleUserCollection.InsertOneAsync(user);
  }

  public async Task<Result> DeleteById(string userId) {

    var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userId);

    var deleteResult = await _googleUserCollection.DeleteOneAsync(filter);

    if(deleteResult.IsAcknowledged is false) return Result.Failure($"Failed to delete user with id {userId}");

    return Result.Success();
  }

  public async Task<Maybe<GoogleUser>> GetByEmail(string email) {

    var filter = Builders<GoogleUser>.Filter.Eq(u => u.Email, email);

    return await _googleUserCollection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Maybe<GoogleUser>> GetById(string userId) {

    var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userId);

    return await _googleUserCollection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<List<GoogleUser>> GetByIds(List<string> userIds) {

    var filter = Builders<GoogleUser>.Filter.In(u => u.Id, userIds);
    
    return await _googleUserCollection.Find(filter).ToListAsync();
  }

  public async Task<Maybe<GoogleUser>> GetBySub(string sub) {
    
    var filter = Builders<GoogleUser>.Filter.Eq(u => u.Sub, sub);

    return await _googleUserCollection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Result> Update(GoogleUser editedUser) {

    var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, editedUser.Id);

    var replaceOneResult = await _googleUserCollection.ReplaceOneAsync(filter, editedUser);

    if(replaceOneResult.IsAcknowledged is false) return Result.Failure($"Failed to update user with id {editedUser.Id}");

    if(replaceOneResult.MatchedCount == 0) return Result.Failure($"User with id {editedUser.Id} has not been found");

    return Result.Success();
  }
}