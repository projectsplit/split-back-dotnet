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

    var roleIDs = new List<ObjectId>();
    roleIDs.AddRange(group.Roles.Where(role => role.Title == "Owner").Select(role => role.Id));

    var client = new MongoClient(_connectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {

      await _groupCollection.InsertOneAsync(group);
      await AddUserToGroup2(group.Id, group.CreatorId, roleIDs);
      await session.CommitTransactionAsync();

    } catch(Exception ex) {

      await session.AbortTransactionAsync();
      Console.WriteLine(ex.Message);
    }

    return Result.Success();
  }
}