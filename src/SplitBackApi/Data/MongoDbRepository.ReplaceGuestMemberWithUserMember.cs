using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> ReplaceGuestMemberWithUserMember(string groupId, UserMember userMember, string guestId) {

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await RemoveMemberFromGroup(groupId, guestId, session);
      await AddUserToGroup(groupId, userMember, session);

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());

    }

    return Result.Success();
  }
}
