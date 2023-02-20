using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RestoreGuestToGroup(string groupId, string userId) {

      var group = await _groupCollection.Find(g => g.Id == groupId).SingleOrDefaultAsync();
      if(group is null) return Result.Failure($"Group {groupId} Not Found");

      var guestToRestore = group.DeletedMembers.Where(g => g.Id == userId).SingleOrDefault();
      if(guestToRestore is null) return Result.Failure($"Guest with UserId {userId} not found");

      group.DeletedMembers.Remove(guestToRestore);
      group.Members.Add(guestToRestore);

      await _groupCollection.ReplaceOneAsync(g => g.Id == groupId, group);

      return Result.Success();
    }
  }