using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result> CreateGroup(Group group) {

    if(group is null) return Result.Failure("CreateGroup error: Group is null");

    group.Roles.Add(_roleService.CreateDefaultRole(ObjectId.GenerateNewId().ToString(), "Everyone"));
    group.Roles.Add(_roleService.CreateDefaultRole(ObjectId.GenerateNewId().ToString(), "Owner"));

    var roleIds = new List<string>();
    roleIds.AddRange(group.Roles.Where(role => role.Title == "Owner").Select(role => role.Id));

    var userMember = new UserMember {
      Id = ObjectId.GenerateNewId().ToString(),
      UserId = group.CreatorId,
      Roles = roleIds
    };

    using var session = await _mongoClient.StartSessionAsync();
    session.StartTransaction();

    try {

      await _groupCollection.InsertOneAsync(session, group);
      await AddUserToGroup(group.Id, userMember, session);

      await session.CommitTransactionAsync();

    } catch(MongoException e) {

      await session.AbortTransactionAsync();
      return Result.Failure(e.ToString());
    }

    return Result.Success();
  }
}