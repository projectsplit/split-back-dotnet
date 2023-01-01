using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Entities;

namespace SplitBackApi.Data;

public class MongoDbRepository : IRepository {

  private readonly MongoDbSettings _dbSettings;
  private readonly IMongoCollection<User> _userCollection;
  private readonly IMongoCollection<Session> _sessionCollection;

  public MongoDbRepository(IOptions<AppSettings> appSettings) {

    _dbSettings = appSettings.Value.MongoDb;

    var mongoClient = new MongoClient(_dbSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(_dbSettings.Database.Name);

    _userCollection = mongoDatabase.GetCollection<User>(_dbSettings.Database.Collections.Users);
    _sessionCollection = mongoDatabase.GetCollection<Session>(_dbSettings.Database.Collections.Sessions);
  }

  public async Task<Session> GetSessionByRefreshToken(string refreshToken) {

    return await _sessionCollection.Find(session => session.RefreshToken == refreshToken).SingleOrDefaultAsync();
  }

  public async Task AddSession(Session session) {
    
    await _sessionCollection.InsertOneAsync(session);
  }

  public async Task AddUser(User user) {
    
    await _userCollection.InsertOneAsync(user);
  }

  public async Task<Session> GetSessionByUnique(string unique) {
    
    return await _sessionCollection.Find(session => session.Unique == unique).SingleOrDefaultAsync();
  }

  public async Task<User> GetUserByEmail(string email) {
    
    var filter = Builders<User>.Filter.Eq("Email", email);
    return await _userCollection.Find(filter).SingleOrDefaultAsync();
  }

  public async Task<bool> UserExistsWithEmail(string email) {
    
    var filter = Builders<User>.Filter.Eq("Email", email);
    var userCount = await _userCollection.CountDocumentsAsync(filter);
    return userCount > 0;
  }

  public async Task<User> GetUserById<ObjectId>(ObjectId userId)
  {
    var filter = Builders<User>.Filter.Eq("_id", userId);
    return await _userCollection.Find(filter).SingleOrDefaultAsync();
  }
}
