using CSharpFunctionalExtensions;

using MongoDB.Driver;


namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> RegenerateUserInvitation(string inviterId, string groupId) {

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await DeleteUserInvitation(inviterId, groupId);
      await CreateUserInvitation(inviterId, groupId);

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());

    }

    return Result.Success();
  }
}


