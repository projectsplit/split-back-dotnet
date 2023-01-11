using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public class MongoDbRepository : IRepository {
  
  private readonly IMongoCollection<Session> _sessionCollection;
  private readonly IMongoCollection<User> _userCollection;
  private readonly IMongoCollection<Group> _groupCollection;

  public MongoDbRepository(IOptions<AppSettings> appSettings) {

    var dbSettings = appSettings.Value.MongoDb;
    var mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(dbSettings.Database.Name);

    _sessionCollection = mongoDatabase.GetCollection<Session>(dbSettings.Database.Collections.Sessions);
    _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Database.Collections.Users);
    _groupCollection = mongoDatabase.GetCollection<Group>(dbSettings.Database.Collections.Groups);
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

  public async Task<User> GetUserById(string userId) {
    var filter = Builders<User>.Filter.Eq("_id", new ObjectId(userId));
    return await _userCollection.Find(filter).SingleOrDefaultAsync();
  }
}
