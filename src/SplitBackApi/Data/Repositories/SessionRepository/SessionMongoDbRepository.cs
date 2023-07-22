using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Data.Repositories.SessionRepository;

public class SessionMongoDbRepository : ISessionRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Session> _sessionCollection;

  public SessionMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _sessionCollection = mongoDatabase.GetCollection<Session>(dbSettings.Database.Collections.Sessions);
  }

  public async Task Create(Session session) {
    await _sessionCollection.InsertOneAsync(session);
  }

  public async Task<Result<Session>> GetByRefreshToken(string refreshToken) {
    var filter = Builders<Session>.Filter.Eq(s => s.RefreshToken, refreshToken);

    var session = await _sessionCollection.Find(filter).SingleOrDefaultAsync();
    if(session is null) return Result.Failure<Session>($"Session with refresh token {refreshToken} has not been found");

    return session;
  }

  public async Task<Result<Session>> GetByUnique(string unique) {

    var filter = Builders<Session>.Filter.And(
       Builders<Session>.Filter.Eq("_t", "JwtAuth"),
       Builders<Session>.Filter.Eq("Unique", unique)
   );
    var session = await _sessionCollection.Find(filter).SingleOrDefaultAsync();

    if(session is null) return Result.Failure<Session>($"Session with unique {unique} has not been found");

    return session;

    
  }

   public async Task<List<Session>> GetLatest(int limit, DateTime lastDateTime)
  {
    var filter = Builders<Session>.Filter.Lt(u => u.CreationTime, lastDateTime);
    var sort = Builders<Session>.Sort.Descending(u => u.CreationTime);

    return await _sessionCollection.Find(filter).Sort(sort).Limit(limit).ToListAsync();
  }
}