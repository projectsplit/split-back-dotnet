using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CreateGroup(Group group) {

    if(group is null) return Result.Failure("CreateGroup error: Group is null");

    group.Roles.Add(_roleService.CreateDefaultRole("Everyone"));
    group.Roles.Add(_roleService.CreateDefaultRole("Owner"));

    var roleIds = new List<ObjectId>();
    roleIds.AddRange(group.Roles.Where(role => role.Title == "Owner").Select(role => role.Id));

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await _groupCollection.InsertOneAsync(session, group);
      await AddUserToGroup(session, group.Id, group.CreatorId, roleIds);
      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }
}