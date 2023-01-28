using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using AutoMapper;
using SplitBackApi.Services;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  private readonly IMongoCollection<Session> _sessionCollection;
  private readonly IMongoCollection<User> _userCollection;
  private readonly IMongoCollection<Group> _groupCollection;
  private readonly IMongoCollection<Invitation> _invitationCollection;
  private readonly string _connectionString;
  private readonly IMapper _mapper;
  private readonly RoleService _roleService;

  public MongoDbRepository(IOptions<AppSettings> appSettings, IMapper mapper, RoleService roleService) {

    var dbSettings = appSettings.Value.MongoDb;
    var mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(dbSettings.Database.Name);

    _sessionCollection = mongoDatabase.GetCollection<Session>(dbSettings.Database.Collections.Sessions);
    _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Database.Collections.Users);
    _groupCollection = mongoDatabase.GetCollection<Group>(dbSettings.Database.Collections.Groups);
    _invitationCollection = mongoDatabase.GetCollection<Invitation>(dbSettings.Database.Collections.Invitations);
    _connectionString = dbSettings.ConnectionString;
    _mapper = mapper;
    _roleService = roleService;
  }
}

