using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> DeleteUserInvitation(string userId, string groupId) {

    var filter =
    Builders<Invitation>.Filter.Eq("Inviter", userId.ToObjectId()) &
    Builders<Invitation>.Filter.Eq("GroupId", groupId.ToObjectId());

    var invitation = await _invitationCollection.DeleteManyAsync(filter);
    if(invitation is null) return Result.Failure($"User Invitation from inviter {userId}, for group {groupId} not found");

    return Result.Success();
  }
}