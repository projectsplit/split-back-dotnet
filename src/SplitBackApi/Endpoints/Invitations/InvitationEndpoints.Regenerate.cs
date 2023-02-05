using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Regenerate(
    IRepository repo,
    InvitationDto invitationDto,
    IOptions<AppSettings> appSettings
  ) {

    var client = new MongoClient(appSettings.Value.MongoDb.ConnectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();

    try {

      await repo.DeleteInvitation(invitationDto.InviterId, invitationDto.GroupId);
      await repo.CreateInvitation(invitationDto.InviterId, invitationDto.GroupId);

      await session.CommitTransactionAsync();

      return Results.Ok();

    } catch(MongoException ex) {

      await session.AbortTransactionAsync();

      return Results.BadRequest(ex.Message);
    }
  }
}