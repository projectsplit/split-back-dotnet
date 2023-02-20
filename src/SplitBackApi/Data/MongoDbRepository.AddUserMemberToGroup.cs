using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data.Extensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> AddUserMemberToGroup(string groupId, string userId, List<string> roleIds) {

    var userMember = new UserMember {
      Id = ObjectId.GenerateNewId().ToString(),
      UserId = userId,
      Roles = roleIds
    };

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {
      await AddUserToGroup(groupId, userMember, session);
      await session.CommitTransactionAsync();

    } catch(MongoException ex) {

      await session.AbortTransactionAsync();
      return Result.Failure(ex.ToString());
    }

    return Result.Success();
  }
}