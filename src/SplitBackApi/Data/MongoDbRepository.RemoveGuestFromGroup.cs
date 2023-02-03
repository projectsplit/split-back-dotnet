using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RemoveGuestFromGroup(ObjectId groupId, ObjectId userId) {

    var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
    if(group is null) return Result.Failure($"Group {groupId} Not Found");

    var guestToRemove = group.Guests.Where(g => g.UserId == userId).SingleOrDefault();
    if(guestToRemove is null) return Result.Failure($"Guest with UserId {userId} not found");

    group.Guests.Remove(guestToRemove);
    group.DeletedGuests.Add(guestToRemove);

    await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);

    return Result.Success();

  }
}