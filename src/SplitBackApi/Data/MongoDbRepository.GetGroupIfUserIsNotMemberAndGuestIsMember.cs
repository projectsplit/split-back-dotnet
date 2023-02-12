using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> GetGroupIfUserIsNotMemberAndGuestIsMember(string userId, Invitation invitation) {

    var guestInvitation = (GuestInvitation)invitation;
    
    var filter =
        Builders<Group>.Filter.Eq("_id", guestInvitation.GroupId.ToObjectId()) &
        Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.Id == guestInvitation.GuestId) &
        Builders<Group>.Filter.Ne("Members.$.UserId", userId.ToObjectId());

    var group = await _groupCollection.Find(filter).SingleOrDefaultAsync();

    if(group is null) return Result.Failure<Group>($"User {userId} already exists, or guest {guestInvitation.GuestId} does not exist in group {guestInvitation.GroupId}");

    return group;
  }
}
