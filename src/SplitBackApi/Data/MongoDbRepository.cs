using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using AutoMapper;
using SplitBackApi.Services;
using CSharpFunctionalExtensions;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  private readonly IMongoCollection<Session> _sessionCollection;
  private readonly IMongoCollection<User> _userCollection;
  private readonly IMongoCollection<Group> _groupCollection;
  private readonly IMongoCollection<Invitation> _invitationCollection;
  //private readonly IMongoCollection<GuestInvitation> _guestInvitationCollection;
  private readonly IMapper _mapper;
  private readonly RoleService _roleService;
  private readonly MongoTransactionService _mongoTransactionService;
  private readonly MongoClient _mongoClient;

  public MongoDbRepository(
    IOptions<AppSettings> appSettings,
    IMapper mapper,
    RoleService roleService,
    MongoTransactionService mongoTransactionService
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _sessionCollection = mongoDatabase.GetCollection<Session>(dbSettings.Database.Collections.Sessions);
    _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Database.Collections.Users);
    _groupCollection = mongoDatabase.GetCollection<Group>(dbSettings.Database.Collections.Groups);
    _invitationCollection = mongoDatabase.GetCollection<Invitation>(dbSettings.Database.Collections.Invitations);

    _mapper = mapper;
    _roleService = roleService;
    _mongoTransactionService = mongoTransactionService;
  }
}

