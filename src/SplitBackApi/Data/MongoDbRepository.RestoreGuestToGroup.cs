using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RestoreGuestToGroup(ObjectId groupId, ObjectId userId) {

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {

      var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var guestToRestore = group.DeletedGuests.Where(g => g.UserId == userId).SingleOrDefault();
      if(guestToRestore is null) return Result.Failure($"Guest with UserId {userId} not found");

      group.DeletedGuests.Remove(guestToRestore);
      group.Guests.Add(guestToRestore);

      await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);
    } catch(Exception) {

      await session.AbortTransactionAsync();
    }

    return Result.Success();
  }
}