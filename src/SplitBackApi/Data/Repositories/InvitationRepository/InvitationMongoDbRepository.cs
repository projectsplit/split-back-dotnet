using SplitBackApi.Domain;
using CSharpFunctionalExtensions;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;

namespace SplitBackApi.Data;

public class InvitationMongoDbRepository : IInvitationRepository {

  private readonly MongoClient _mongoClient;
  private readonly IMongoCollection<Invitation> _invitationCollection;

  public InvitationMongoDbRepository(
    IOptions<AppSettings> appSettings
  ) {

    var dbSettings = appSettings.Value.MongoDb;
    _mongoClient = new MongoClient(dbSettings.ConnectionString);
    var mongoDatabase = _mongoClient.GetDatabase(dbSettings.Database.Name);

    _invitationCollection = mongoDatabase.GetCollection<Invitation>(dbSettings.Database.Collections.Invitations);
  }

  public async Task Create(Invitation invitation) {

    await _invitationCollection.InsertOneAsync(invitation);
  }

  public async Task<Result> DeleteById(string invitationId) {

    var filter = Builders<Invitation>.Filter.Eq(i => i.Id, invitationId);

    await _invitationCollection.DeleteOneAsync(filter);

    return Result.Success();
  }

  public async Task<Result<Invitation>> GetById(string invitationId) {

    var filter = Builders<Invitation>.Filter.Eq(i => i.Id, invitationId);

    var invitation = await _invitationCollection.Find(filter).FirstOrDefaultAsync();
    if(invitation is null) return Result.Failure<Invitation>($"Invitation with id {invitationId} has not been found");

    return invitation;
  }

  public async Task<Result<Invitation>> GetByCode(string code) {

    var filter = Builders<Invitation>.Filter.Eq(i => i.Code, code);

    var invitation = await _invitationCollection.Find(filter).SingleOrDefaultAsync();
    if(invitation is null) return Result.Failure<Invitation>($"Invitation with id {code} has not been found");

    return invitation;
  }

  public async Task<ICollection<Invitation>> GetByInviterId(string inviterId) {

    var filter = Builders<Invitation>.Filter.Eq(i => i.InviterId, inviterId);

    var invitations = await _invitationCollection.Find(filter).ToListAsync();

    return invitations;
  }

  public async Task<Result> Update(Invitation editedInvitation) {

    var filter = Builders<Invitation>.Filter.Eq(i => i.Id, editedInvitation.Id);

    var replaceOneResult = await _invitationCollection.ReplaceOneAsync(filter, editedInvitation);

    if(replaceOneResult.IsAcknowledged is false) return Result.Failure($"Failed to update invitation with id {editedInvitation.Id}");

    if(replaceOneResult.MatchedCount == 0) return Result.Failure($"Invitation with id {editedInvitation.Id} has not been found");

    return Result.Success();
  }
}