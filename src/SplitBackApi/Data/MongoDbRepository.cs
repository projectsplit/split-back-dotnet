using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Configuration;
using SplitBackApi.Domain;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using AutoMapper;

namespace SplitBackApi.Data;

public class MongoDbRepository : IRepository
{
    private readonly IMongoCollection<Session> _sessionCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Group> _groupCollection;
    private readonly IMongoCollection<Invitation> _invitationCollection;
    private readonly string _connectionString;
    private readonly IMapper _mapper;

    public MongoDbRepository(IOptions<AppSettings> appSettings, IMapper mapper)
    {

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

    public async Task<Session> GetSessionByRefreshToken(string refreshToken)
    {
        return await _sessionCollection.Find(session => session.RefreshToken == refreshToken).SingleOrDefaultAsync();
    }

    public async Task AddSession(Session session)
    {
        await _sessionCollection.InsertOneAsync(session);
    }

    public async Task AddUser(User user)
    {
        await _userCollection.InsertOneAsync(user);
    }

    public async Task<Session> GetSessionByUnique(string unique)
    {
        return await _sessionCollection.Find(session => session.Unique == unique).SingleOrDefaultAsync();
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var filter = Builders<User>.Filter.Eq("Email", email);
        return await _userCollection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task<bool> UserExistsWithEmail(string email)
    {
        var filter = Builders<User>.Filter.Eq("Email", email);
        var userCount = await _userCollection.CountDocumentsAsync(filter);
        return userCount > 0;
    }

    public async Task<User> GetUserById(string userId)
    {
        var filter = Builders<User>.Filter.Eq("_id", new ObjectId(userId));
        return await _userCollection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task<bool> EmailExists(string email)
    {
        //var filter = Builders<User>.Filter.Eq("Email", email);
        var userCount = await _userCollection.CountDocumentsAsync(user => user.Email == email);
        return userCount > 0;
    }



    public async Task<User> GetUserById(ObjectId userId)
    {
        return await _userCollection.Find(user => user.Id == userId).SingleOrDefaultAsync();
    }

    public Task AddLabel(Label label)
    {
        throw new NotImplementedException();
    }

    public async Task AddNewExpense(NewExpenseDto newExpenseDto)
    {
        var newExpense = _mapper.Map<Expense>(newExpenseDto);
        newExpense.CreationTime = DateTime.Now;

        var groupId = ObjectId.Parse(newExpenseDto.GroupId);
        var updateExpenses = Builders<Group>.Update.AddToSet("Expenses", newExpense);
        await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateExpenses);
    }
    public async Task EditExpense(EditExpenseDto editExpenseDto)
    {
        var newExpense = _mapper.Map<Expense>(editExpenseDto);
        var groupId = ObjectId.Parse(editExpenseDto.GroupId);
        var expenseId = ObjectId.Parse(editExpenseDto.ExpenseId);
        //var expenseId = ObjectId.Parse("63ac1e064b49cf6ddbf27738");
        var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
        var updateExpense = Builders<Group>.Update
          .Set("Expenses.$.Description", newExpense.Description)
          .Set("Expenses.$.Amount", newExpense.Amount)
          .Set("Expenses.$.ExpenseSpenders", newExpense.ExpenseSpenders)
          .Set("Expenses.$.ExpenseParticipants", newExpense.Participants)
          .Set("Expenses.$.Label", newExpense.Labels)
          .Set("Expenses.$.IsoCode", newExpense.IsoCode);

        var client = new MongoClient(_connectionString);
        using var session = await client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
            await AddToHistory(oldGroup, expenseId, filter, true);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            Console.WriteLine(ex.Message);
        }
    }

    public async Task AddComment(NewCommentDto comment, ObjectId userId)
    {
        var newComment = _mapper.Map<Comment>(comment);
        newComment.CommentorId = userId;
        var expenseId = ObjectId.Parse(comment.ExpenseId);
        var groupId = ObjectId.Parse(comment.GroupId);
        var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
        var updateExpense = Builders<Group>.Update.Push("Expenses.$.Comments", newComment);
        try
        {
            await _groupCollection.FindOneAndUpdateAsync(filter, updateExpense);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task AddNewTransfer(NewTransferDto newTransferDto)
    {
        var newTransfer = _mapper.Map<Transfer>(newTransferDto);
        newTransfer.CreationTime = DateTime.Now;

        var groupId = ObjectId.Parse(newTransferDto.GroupId);
        var updateTransfers = Builders<Group>.Update.AddToSet("Transfers", newTransfer);
        await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupId, updateTransfers);
    }
    public async Task AddToHistory(Group oldGroup, ObjectId OperationId, FilterDefinition<Group>? filter, bool isExpense)
    {
        if (isExpense)
        {
            var oldExpense = oldGroup.Expenses.First(e => e.Id == OperationId);
            var snapShot = _mapper.Map<ExpenseSnapshot>(oldExpense);
            var update = Builders<Group>.Update.Push("Expenses.$.History", snapShot);
            await _groupCollection.FindOneAndUpdateAsync(filter, update);
        }
        else
        {
            var oldTransfer = oldGroup.Transfers.First(t => t.Id == OperationId);
            var snapShot = _mapper.Map<TransferSnapshot>(oldTransfer);
            var update = Builders<Group>.Update.Push("Transfers.$.History", snapShot);
            await _groupCollection.FindOneAndUpdateAsync(filter, update);
        }
    }

    public async Task EditTransfer(EditTransferDto editTransferDto)
    {
        var newTransfer = _mapper.Map<Transfer>(editTransferDto);
        var groupId = ObjectId.Parse(editTransferDto.GroupId);
        var transferId = ObjectId.Parse(editTransferDto.TransferId);
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
        try
        {
            var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, updateTransfer);
            await AddToHistory(oldGroup, transferId, filter, false);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            Console.WriteLine(ex.Message);
        }
    }

    public async Task RemoveOrRestoreExpense(RemoveRestoreExpenseDto removeRestoreExpenseDto)
    {
        try
        {
            var groupId = ObjectId.Parse(removeRestoreExpenseDto.GroupId);
            var expenseId = ObjectId.Parse(removeRestoreExpenseDto.ExpenseId);
            var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Expenses, e => e.Id == expenseId);
            if (removeRestoreExpenseDto.Remove == true)
            {
                var expense = Builders<Group>.Update.Set("Expenses.$.IsDeleted", true);
                var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, expense);
            }
            else if (removeRestoreExpenseDto.Remove == false)
            {
                var expense = Builders<Group>.Update.Set("Expenses.$.IsDeleted", false);
                var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, expense);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task RemoveOrRestoreTransfer(RemoveRestoreTransferDto removeRestoreTransferDto)
    {
        try
        {
            var groupId = ObjectId.Parse(removeRestoreTransferDto.GroupId);
            var transferId = ObjectId.Parse(removeRestoreTransferDto.TransferId);
            var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.ElemMatch(g => g.Transfers, e => e.Id == transferId);
            if (removeRestoreTransferDto.Remove == true)
            {
                var removeExpense = Builders<Group>.Update.Set("Transfers.$.IsDeleted", true);
                var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, removeExpense);
            }
            else if (removeRestoreTransferDto.Remove == false)
            {
                var removeExpense = Builders<Group>.Update.Set("Transfers.$.IsDeleted", false);
                var oldGroup = await _groupCollection.FindOneAndUpdateAsync(filter, removeExpense);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task AddUserToGroup(ObjectId groupID, ObjectId userID)
    {
        var filter = Builders<Group>.Filter.Eq("_id", groupID) & Builders<Group>.Filter.AnyEq("Members.$.UserId", userID);
        var userCount = await _groupCollection.CountDocumentsAsync(filter);

        if (userCount == 0)
        {
            //update group
            var updateGroup = Builders<Group>.Update.AddToSet("Members", userID);
            await _groupCollection.FindOneAndUpdateAsync(group => group.Id == groupID, updateGroup);
            //update user
            var updateUser = Builders<User>.Update.AddToSet("Groups", groupID);
            await _userCollection.FindOneAndUpdateAsync(user => user.Id == userID, updateUser);
        }
        else throw new Exception();
    }

    public async Task CreateGroup(Group group)
    {
        if (group is null)
        {
            throw new ArgumentNullException(nameof(group));
        }
        var client = new MongoClient(_connectionString);
        using var session = await client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            await _groupCollection.InsertOneAsync(group);
            await AddUserToGroup(group.Id, group.CreatorId);
            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            Console.WriteLine(ex.Message);
        }
    }
    public async Task<Group?> GetGroupById(ObjectId groupId)
    {
        return await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId)).FirstOrDefaultAsync();
    }

    public async Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId)
    {
        return await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId)).FirstOrDefaultAsync();
    }

    public async Task<Invitation> GetInvitationByCode(string Code)
    {
        return await _invitationCollection.Find(Builders<Invitation>.Filter.Eq("Code", Code)).FirstOrDefaultAsync();
    }
    public async Task CreateInvitation(ObjectId inviterId, ObjectId groupId)
    {
        var invitation = new Invitation
        {
            Code = InvitationCodeGenerator.GenerateInvitationCode(),
            GroupId = groupId,
            Inviter = inviterId,
            CreationTime = DateTime.UtcNow
        };
        await _invitationCollection.InsertOneAsync(invitation);
    }

    public async Task<Group> CheckIfUserInGroupMembers(ObjectId userId, ObjectId groupId)
    {
        return await _groupCollection.Find(Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members", userId)).FirstOrDefaultAsync();
    }
    public async Task<Group> CheckAndAddUserInGroupMembers(ObjectId userId, ObjectId groupId)
    {
        var filter = Builders<Group>.Filter.Eq("_id", groupId) & Builders<Group>.Filter.Ne("Members", userId);
        var updateGroup = Builders<Group>.Update.AddToSet("Members", userId);
        return await _groupCollection.FindOneAndUpdateAsync(filter, updateGroup);
    }
    public async Task<User> CheckIfGroupInUser(ObjectId userId, ObjectId groupId)
    {
        return await _userCollection.Find(Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId)).FirstOrDefaultAsync();
    }

    public async Task<User> CheckAndAddGroupInUser(ObjectId userId, ObjectId groupId)
    {
        var filter = Builders<User>.Filter.Eq("_id", userId) & Builders<User>.Filter.Ne("Groups", groupId);
        var updateUser = Builders<User>.Update.AddToSet("Groups", groupId);
        return await _userCollection.FindOneAndUpdateAsync(filter, updateUser);
    }
    public async Task<DeleteResult> DeleteInvitation(ObjectId userId, ObjectId groupId)
    {
        return await _invitationCollection.DeleteManyAsync(Builders<Invitation>.Filter.Eq("Inviter", userId) & Builders<Invitation>.Filter.Eq("GroupId", groupId));
    }

}
