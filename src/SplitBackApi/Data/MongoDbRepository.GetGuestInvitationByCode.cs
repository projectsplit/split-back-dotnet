using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {
  
  public async Task<Result<Invitation>> GetGuestInvitationByCode(string Code) {

    var filter = Builders<Invitation>.Filter.Eq("Code", Code);
    
    var guestInvitation = await _invitationCollection.Find(filter).SingleOrDefaultAsync();
    
    if(guestInvitation is null) return Result.Failure<Invitation>($"Invitation with code {Code} not found");
    
    return guestInvitation;
  }
}