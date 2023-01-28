using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Invitation> GetInvitationByInviter(ObjectId userId, ObjectId groupId) {

    var filter =
      Builders<Invitation>.Filter.Eq("Inviter", userId) &
      Builders<Invitation>.Filter.Eq("GroupId", groupId);

    return await _invitationCollection.Find(filter).FirstOrDefaultAsync();
  }
}