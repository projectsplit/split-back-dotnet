using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using AutoMapper;
using LanguageExt.Common;
using LanguageExt;

namespace SplitBackApi.Data;

public class MongoDbRepository : IRepository {
  private readonly IMongoCollection<Session> _sessionCollection;
  private readonly IMongoCollection<User> _userCollection;
  private readonly IMongoCollection<Group> _groupCollection;
  private readonly IMongoCollection<Invitation> _invitationCollection;
  private readonly string _connectionString;
  private readonly IMapper _mapper;

  public MongoDbRepository(IOptions<AppSettings> appSettings, IMapper mapper) {

    var dbSettings = appSettings.Value.MongoDb;
    var mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(dbSettings.Database.Name);

    _sessionCollection = mongoDatabase.GetCollection<Session>(dbSettings.Database.Collections.Sessions);
    _userCollection = mongoDatabase.GetCollection<User>(dbSettings.Database.Collections.Users);
    _groupCollection = mongoDatabase.GetCollection<Group>(dbSettings.Database.Collections.Groups);
    _invitationCollection = mongoDatabase.GetCollection<Invitation>(dbSettings.Database.Collections.Invitations);
    _connectionString = dbSettings.ConnectionString;
    _mapper = mapper;
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

  public async Task<bool> EmailExists(string email) {
    //var filter = Builders<User>.Filter.Eq("Email", email);
    var userCount = await _userCollection.CountDocumentsAsync(user => user.Email == email);
    return userCount > 0;
  }



  public async Task<User> GetUserById(ObjectId userId) {
    return await _userCollection.Find(user => user.Id == userId).SingleOrDefaultAsync();
  }

  public Task AddLabel(Label label) {
    throw new NotImplementedException();
  }

  public async Task AddNewExpense(Expense newExpense, ObjectId groupId) {

    var updateExpenses = Builders<Group>.Update.AddToSet("Expenses", newExpense);
    await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateExpenses);
  }

  public async Task<Result<Unit>> EditExpense(Expense newExpense, ObjectId groupId, ObjectId expenseId) {

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
      if(oldGroup is null) return new Result<Unit>(new EditExpenseException("Group not found"));

      await AddExpenseToHistory(oldGroup, expenseId, filter);

    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }

    return new Unit();

  }


  public async Task AddNewTransfer(Transfer newTransfer, ObjectId groupId) {

    var updateTransfers = Builders<Group>.Update.AddToSet("Transfers", newTransfer);
    await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateTransfers);
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



  public async Task EditTransfer(Transfer newTransfer, ObjectId groupId, ObjectId transferId) {

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
      await AddTransferToHistory(oldGroup, transferId, filter);
    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }
  }


  public async Task RemoveExpense(string groupId, string expenseId) {
    try {
      await UpdateExpenseIsDeleted(groupId, expenseId, true);
    } catch(Exception ex) {
      Console.WriteLine(ex.Message);
    }
  }

  public async Task RestoreExpense(string groupId, string expenseId) {
    try {
      await UpdateExpenseIsDeleted(groupId, expenseId, false);
    } catch(Exception ex) {
      Console.WriteLine(ex.Message);
    }
  }


  public async Task AddComment(Comment newComment, ObjectId expenseId, ObjectId groupId) {

    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
    var updateExpense = Builders<Group>.Update.Push("Expenses.$.Comments", newComment);
    try {
      await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
    } catch(Exception ex) {
      Console.WriteLine(ex.Message);
    }
  }
  // var filter = Builders<Group>.Filter.Eq("_id", groupBsonId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseBsonId);
  // var expense = Builders<Group>.Update.Set("Expenses.$.IsDeleted", remove);
  // var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, expense);
  private async Task UpdateExpenseIsDeleted(string groupId, string expenseId, bool remove) {

    var groupBsonId = ObjectId.Parse(groupId);
    var expenseBsonId = ObjectId.Parse(expenseId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).FirstOrDefaultAsync();
      switch(remove) {
        case true:
          var expenseToRemove = group.Expenses.Where(e => e.Id == expenseBsonId).First();
          group.Expenses.Remove(expenseToRemove);
          group.DeletedExpenses.Add(expenseToRemove);
          await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
          break;
        case false:
          var expenseToRestore = group.DeletedExpenses.Where(e => e.Id == expenseBsonId).First();
          group.DeletedExpenses.Remove(expenseToRestore);
          group.Expenses.Add(expenseToRestore);
          await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
          break;
      }
      await session.CommitTransactionAsync();
    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }

  }

  public async Task RemoveTransfer(string groupId, string transferId) {
    try {
      await UpdateTransferIsDeleted(groupId, transferId, true);
    } catch(Exception ex) {
      Console.WriteLine(ex.Message);
    }
  }

  public async Task RestoreTransfer(string groupId, string transferId) {
    try {
      await UpdateTransferIsDeleted(groupId, transferId, false);
    } catch(Exception ex) {
      Console.WriteLine(ex.Message);
    }
  }

  private async Task UpdateTransferIsDeleted(string groupId, string transferId, bool remove) {
    var groupBsonId = ObjectId.Parse(groupId);
    var transferBsonId = ObjectId.Parse(transferId);

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {
      var group = await _groupCollection.Find(g => g.Id == groupBsonId).FirstOrDefaultAsync();
      switch(remove) {
        case true:
          var transferToRemove = group.Transfers.Where(t => t.Id == transferBsonId).First();
          group.Transfers.Remove(transferToRemove);
          group.DeletedTransfers.Add(transferToRemove);
          await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
          break;
        case false:
          var transferToRestore = group.DeletedTransfers.Where(t => t.Id == transferBsonId).First();
          group.DeletedTransfers.Remove(transferToRestore);
          group.Transfers.Add(transferToRestore);
          await _groupCollection.ReplaceOneAsync(g => g.Id == groupBsonId, group);
          break;
      }
      await session.CommitTransactionAsync();
    } catch(Exception _) {
      await session.AbortTransactionAsync();
    }
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

  public async Task CreateGroup(Group group) {
    if(group is null) {
      throw new ArgumentNullException(nameof(group));
    }
    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      await _groupCollection.InsertOneAsync(group);
      await AddUserToGroup(group.Id, group.CreatorId);
      await session.CommitTransactionAsync();
    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }
  }
  public async Task<Result<Group>> GetGroupById(ObjectId groupId) {

    var group = await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId)).FirstOrDefaultAsync();
    if(group is null) return new Result<Group>(new GroupNotFoundException($"Group {groupId} Not Found"));
    return group;

  }

  public async Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId) {
    return await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId)).FirstOrDefaultAsync();
  }

  public async Task<Invitation> GetInvitationByCode(string Code) {
    return await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Code", Code)).FirstOrDefaultAsync();
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

  public async Task<Group> CheckIfUserInGroupMembers(ObjectId userId, ObjectId groupId) {
    return await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members", userId)).FirstOrDefaultAsync();
  }
  public async Task<Group> CheckAndAddUserInGroupMembers(ObjectId userId, ObjectId groupId) {
    var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members", userId);
    var updateGroup = Builders<Group>.Update.AddToSet("Members", userId);
    return await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
  }
  public async Task<User> CheckIfGroupInUser(ObjectId userId, ObjectId groupId) {
    return await _userCollection.Find(Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId)).FirstOrDefaultAsync();
  }

  public async Task<User> CheckAndAddGroupInUser(ObjectId userId, ObjectId groupId) {
    var filter = Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId);
    var updateUser = Builders<User>.Update.AddToSet("Groups", groupId);
    return await _userCollection.FindOneAndUpdateAsync(filter, updateUser);
  }
  public async Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId) {
    return await _invitationCollection.DeleteManyAsync(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId));
  }

}
