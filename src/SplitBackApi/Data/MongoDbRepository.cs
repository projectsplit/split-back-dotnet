using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using SplitBackApi.Helper;
using AutoMapper;
using CSharpFunctionalExtensions;
using SplitBackApi.Services;

namespace SplitBackApi.Data;

public class MongoDbRepository : IRepository {
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

  // public async Task<User> GetUserById(string userId) {
  //   var filter = Builders<User>.Filter.Eq("_id", new ObjectId(userId));
  //   return await _userCollection.Find(filter).SingleOrDefaultAsync();
  // }

  public async Task<bool> EmailExists(string email) {
    //var filter = Builders<User>.Filter.Eq("Email", email);
    var userCount = await _userCollection.CountDocumentsAsync(user => user.Email == email);
    return userCount > 0;
  }

  public async Task<Result<User>> GetUserById(ObjectId userId) {
    var user = await _userCollection.Find(user => user.Id == userId).SingleOrDefaultAsync();
    if(user is null) return Result.Failure<User>($"user {userId} not found");
    return user;
  }

  public Task AddLabel(Label label) {
    throw new NotImplementedException();
  }

  public async Task<Result> AddNewExpense(Expense newExpense, ObjectId groupId) {

    var updateExpenses = Builders<Group>.Update.AddToSet("Expenses", newExpense);
    var group = await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateExpenses);
    if(group is null) return Result.Failure($"Group with id {groupId} not found");
    return Result.Success();
  }

  public async Task<Result> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId) {

    //var expenseId = ObjectId.Parse("63ac1e064b49cf6ddbf27738");
    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
    var updateExpense = Builders<Group>.Update
      .Set("Expenses.$.Description", newExpense.Description)
      .Set("Expenses.$.Amount", newExpense.Amount)
      .Set("Expenses.$.Spenders", newExpense.Spenders)
      .Set("Expenses.$.Participants", newExpense.Participants)
      //.Set("Expenses.$.Label", newExpense.Labels)
      .Set("Expenses.$.IsoCode", newExpense.IsoCode);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {

      var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
      if(oldGroup is null) return Result.Failure("Group not found");

      await AddExpenseToHistory(oldGroup, expenseId, filter);

    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();
  }


  public async Task<Result> AddNewTransfer(Transfer newTransfer, ObjectId groupId) {

    var updateTransfers = Builders<Group>.Update.AddToSet("Transfers", newTransfer);
    var group = await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateTransfers);
    if(group is null) return Result.Failure($"Group with id {groupId} not found");
    return Result.Success();
  }

  public async Task AddExpenseToHistory(Group oldGroup, ObjectId OperationId, FilterDefinition<Group>? filter) {

    var oldExpense = oldGroup.Expenses.First(e => e.Id == OperationId);
    var snapShot = _mapper.Map<ExpenseSnapshot>(oldExpense);
    var update = Builders<Group>.Update.Push("Expenses.$.History", snapShot);
    await _groupCollection.FindOneAndUpdateAsync(filter, update);
  }

  public async Task AddTransferToHistory(Group oldGroup, ObjectId OperationId, FilterDefinition<Group>? filter) {

    var oldTransfer = oldGroup.Transfers.First(t => t.Id == OperationId);
    var snapShot = _mapper.Map<TransferSnapshot>(oldTransfer);
    var update = Builders<Group>.Update.Push("Transfers.$.History", snapShot);
    await _groupCollection.FindOneAndUpdateAsync(filter, update);
  }



  public async Task<Result> EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId) {

    //var transferId = ObjectId.Parse("63aafa3ad36b483e99735bcd");
    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Transfers, t => t.Id == transferId);
    var updateTransfer = Builders<Group>.Update
       .Set("Transfers.$.Description", newTransfer.Description)
       .Set("Transfers.$.Amount", newTransfer.Amount)
       .Set("Transfers.$.SenderId", newTransfer.SenderId)
       .Set("Transfers.$.ReceiverId", newTransfer.ReceiverId)
       .Set("Transfers.$.IsoCode", newTransfer.IsoCode);
    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, updateTransfer);
      if(oldGroup is null) return Result.Failure("Group not found");
      await AddTransferToHistory(oldGroup, transferId, filter);
    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }
    return Result.Success();
  }

  public async Task<Result> RemoveExpense(string groupId, string expenseId) {

    var groupBsonId = ObjectId.Parse(groupId);
    var expenseBsonId = ObjectId.Parse(expenseId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var expenseToRemove = group.Expenses.Where(e => e.Id == expenseBsonId).SingleOrDefault();
      if(expenseToRemove is null) return Result.Failure($"Expense with id {expenseId} not found");
      group.Expenses.Remove(expenseToRemove);
      group.DeletedExpenses.Add(expenseToRemove);
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);

      await session.CommitTransactionAsync();
    } catch(Exception) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();

  }

  public async Task<Result> RestoreExpense(string groupId, string expenseId) {

    var groupBsonId = ObjectId.Parse(groupId);
    var expenseBsonId = ObjectId.Parse(expenseId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var expenseToRestore = group.DeletedExpenses.Where(e => e.Id == expenseBsonId).SingleOrDefault();
      if(expenseToRestore is null) return Result.Failure($"Expense with id {expenseId} not found");
      group.DeletedExpenses.Remove(expenseToRestore);
      group.Expenses.Add(expenseToRestore);
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
      await session.CommitTransactionAsync();

    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();
  }


  public async Task<Result> AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
    var updateExpense = Builders<Group>.Update.Push("Expenses.$.Comments", newComment);
    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
    if(group is null) return Result.Failure($"expense {expenseId} in group {groupId} not found");
    return Result.Success();

  }

  public async Task<Result> RemoveTransfer(string groupId, string transferId) {
    var groupBsonId = ObjectId.Parse(groupId);
    var transferBsonId = ObjectId.Parse(transferId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var transferToRemove = group.Transfers.Where(t => t.Id == transferBsonId).SingleOrDefault();
      if(transferToRemove is null) return Result.Failure($"Transfer with id {transferId} not found");
      group.Transfers.Remove(transferToRemove);
      group.DeletedTransfers.Add(transferToRemove);
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);

      await session.CommitTransactionAsync();
    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();
  }

  public async Task<Result> RestoreTransfer(string groupId, string transferId) {
    var groupBsonId = ObjectId.Parse(groupId);
    var transferBsonId = ObjectId.Parse(transferId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var transferToRestore = group.DeletedTransfers.Where(t => t.Id == transferBsonId).SingleOrDefault();
      if(transferToRestore is null) return Result.Failure($"Transfer with id {transferId} not found");

      group.DeletedTransfers.Remove(transferToRestore);
      group.Transfers.Add(transferToRestore);
      await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
      await session.CommitTransactionAsync();

    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
    return Result.Success();
  }


  public async Task AddUserToGroup(ObjectId groupID, ObjectId userID) {
    var filter = Builders<Group>.Filter.Eq("_id", groupID) & Builders<Group>.Filter.AnyEq("Members.$.UserId", userID);
    var userCount = await _groupCollection.CountDocumentsAsync(filter);


    if(userCount == 0) {
      //update group
      var updateGroup = Builders<Group>.Update.AddToSet("Members", userID);
      await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupID, updateGroup);
      //update user
      var updateUser = Builders<User>.Update.AddToSet("Groups", groupID);
      await _userCollection.FindOneAndUpdateAsync(user => user.Id == userID, updateUser);
    } else throw new Exception();
  }

  public async Task AddUserToGroup2(ObjectId groupID, ObjectId userID, ICollection<ObjectId> roleIDs) {
    var filter = Builders<Group>.Filter.Eq("_id", groupID) & Builders<Group>.Filter.ElemMatch(x => x.Members, member => member.UserId == userID);
    var userCount = await _groupCollection.CountDocumentsAsync(filter);

    var member = new Member {
      UserId = userID,
      Roles = roleIDs
    };

    if(userCount == 0) {
      //update group
      var updateGroup = Builders<Group>.Update.AddToSet("Members", member);
      await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupID, updateGroup);
      //update user
      var updateUser = Builders<User>.Update.AddToSet("Groups", groupID);
      await _userCollection.FindOneAndUpdateAsync(user => user.Id == userID, updateUser);
    } else throw new Exception();
  }

  public async Task<Result> CreateGroup(Group group) {
    if(group is null) return Result.Failure("CreateGroup error: Group is null");

    group.Roles.Add(_roleService.CreateDefaultRole("Everyone"));
    group.Roles.Add(_roleService.CreateDefaultRole("Owner"));

    var roleIDs = new List<ObjectId>();
    roleIDs.AddRange(group.Roles.Where(role => role.Title == "Owner").Select(role => role.Id));

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      await _groupCollection.InsertOneAsync(group);
      await AddUserToGroup2(group.Id, group.CreatorId, roleIDs);
      await session.CommitTransactionAsync();
    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }
    return Result.Success();
  }
  public async Task<Result<Group>> GetGroupById(ObjectId groupId) {

    var group = await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId)).FirstOrDefaultAsync();
    if(group is null) return Result.Failure<Group>($"Group with id {groupId} not found");
    return group;

  }

  public async Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId) {
    return await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId)).FirstOrDefaultAsync();
  }

  public async Task<Result<Invitation>> GetInvitationByCode(string Code) {

    var invitation = await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Code", Code)).SingleOrDefaultAsync();
    if(invitation is null) return Result.Failure<Invitation>($"Invitation with code {Code} not found");
    return invitation;
  }
  public async Task CreateInvitation(ObjectId inviterId, ObjectId groupId) {
    var invitation = new Invitation {
      Code = InvitationCodeGenerator.GenerateInvitationCode(),
      GroupId = groupId,
      Inviter = inviterId,
      CreationTime = DateTime.UtcNow
    };
    await _invitationCollection.InsertOneAsync(invitation);
  }

  public async Task<Result<Group>> CheckIfUserInGroupMembers(ObjectId userId, ObjectId groupId) {
    var group = await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members", userId)).SingleOrDefaultAsync();
    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");
    return group;
  }
  public async Task<Result<Group>> CheckAndAddUserInGroupMembers(ObjectId userId, ObjectId groupId) {
    var newMember = new Member { UserId = userId, Roles = new List<ObjectId>() };

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members.$.UserId", userId);
    var updateGroup = Builders<Group>.Update.AddToSet("Members", newMember);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);

    if(group is null) return Result.Failure<Group>($"User {userId} already exists in group {groupId}");
    return group;
  }
  public async Task<Result<User>> CheckIfGroupInUser(ObjectId userId, ObjectId groupId) {
    var user = await _userCollection.Find(Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId)).SingleOrDefaultAsync();
    if(user is null) return Result.Failure<User>($"Group {groupId} already exists in user {userId}");
    return user;
  }

  public async Task<Result<User>> CheckAndAddGroupInUser(ObjectId userId, ObjectId groupId) {
    var filter = Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId);
    var updateUser = Builders<User>.Update.AddToSet("Groups", groupId);
    var user = await _userCollection.FindOneAndUpdateAsync(filter, updateUser);
    if(user is null) return Result.Failure<User>($"Group {groupId} already exists in user {userId}");
    return user;
  }
  public async Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId) {
    return await _invitationCollection.DeleteManyAsync(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId));
  }

  public async Task<Result> CreateRole(ObjectId groupId, string roleName, Role newRole) {
    var filter = Builders<Group>.Filter.Eq("_id", groupId);
    var updateGroup = Builders<Group>.Update.Push("Roles", newRole);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }

  public async Task<Result> EditRole(ObjectId roleId, ObjectId groupId, string roleName, Role newRole) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Roles, r => r.Id == roleId);
    var updateGroup = Builders<Group>.Update
    .Set("Roles.$.Title", newRole.Title)
    .Set("Roles.$.Permissions", newRole.Permissions);

    //var group = _groupCollection.Find<Group>(g => g.Id == groupId).ToList();
    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }

  public async Task<Result> AddRoleToUser(ObjectId groupId, ObjectId userId, ObjectId roleId) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.UserId == userId);
    var updateGroup = Builders<Group>.Update.AddToSet("Members.$.Roles", roleId);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }

    public async Task<Result> RemoveRoleFromUser(ObjectId groupId, ObjectId userId, ObjectId roleId) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.UserId == userId);
    var updateGroup = Builders<Group>.Update.Pull("Members.$.Roles", roleId);

    var group = await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    if(group is null) return Result.Failure($"Group {groupId} not found");

    return Result.Success();
  }
}

