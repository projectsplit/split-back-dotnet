using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using AutoMapper;
using SplitBackApi.Services;
using CSharpFunctionalExtensions;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Helper;

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

  private async Task<Result> RemoveMemberFromGroup(string groupId, string memberId, IClientSessionHandle session) {

    var groupFilter = Builders<Group>.Filter.Where(g => g.Id == groupId);
    var memberFilter = Builders<Member>.Filter.Where(m => m.Id == memberId);

    var update = Builders<Group>.Update.PullFilter(g => g.Members, memberFilter);

    var group = await _groupCollection.UpdateOneAsync(session, groupFilter, update);
    //if(group is null) return Result.Failure($"Group {groupId} not found")

    return Result.Success();
  }

  private async Task AddUserToGroup(string groupId, UserMember userMember, IClientSessionHandle session) {

    var groupFilter =
      Builders<Group>.Filter.Eq("_id", groupId.ToObjectId()) &
      Builders<Group>.Filter.Ne("Members.$.UserId", userMember.UserId);

    var updateGroup = Builders<Group>.Update.AddToSet("Members", userMember);

    var userFilter =
      Builders<User>.Filter.Eq("_id", userMember.UserId.ToObjectId()) &
      Builders<User>.Filter.Ne("Groups", groupId.ToObjectId());

    var userUpdate = Builders<User>.Update.AddToSet("Groups", groupId);

    await _groupCollection.UpdateOneAsync(session, groupFilter, updateGroup);
    // if(updateGroupResult.ModifiedCount == 0) {

    //   await session.AbortTransactionAsync();
    //   return Result.Failure<Group>($"User {userId} already exists in group {groupId}");
    // }

    await _userCollection.UpdateOneAsync(session, userFilter, userUpdate);
    // if(user is null) {

    //   await session.AbortTransactionAsync();
    //   return Result.Failure<User>($"Group {groupId} already exists in user {userId}");
    // }
  }

  private async Task<Result> DeleteUserInvitation(string userId, string groupId) {

    var filter =
      Builders<Invitation>.Filter.Eq("Inviter", userId.ToObjectId()) &
      Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId());

    var invitation = await _invitationCollection.DeleteManyAsync(filter);
    if(invitation is null) return Result.Failure($"User Invitation from inviter {userId}, for group {groupId} not found");

    return Result.Success();
  }
}
