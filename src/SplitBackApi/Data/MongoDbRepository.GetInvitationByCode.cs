using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result<Invitation>> GetInvitationByCode(string Code) {

    var filter = Builders<Invitation>.Filter.Eq("Code", Code);
    
    var invitation = await _invitationCollection.Find(filter).SingleOrDefaultAsync();
    if(invitation is null) return Result.Failure<Invitation>($"Invitation with code {Code} not found");
    
    return invitation;
  }
}