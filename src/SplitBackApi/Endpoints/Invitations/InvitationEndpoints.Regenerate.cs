using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Endpoints.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;
public static partial class InvitationEndpoints {
  private static async Task<IResult> Regenerate(IRepository repo, InvitationDto invitationDto, IOptions<AppSettings> appSettings) {

    var client = new MongoClient(appSettings.Value.MongoDb.ConnectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      var inviterID = ObjectId.Parse(invitationDto.InviterId);
      var groupID = ObjectId.Parse(invitationDto.GroupId);
      await repo.DeleteInvitation(inviterID, groupID);
      await repo.CreateInvitation(inviterID, groupID);
      await session.CommitTransactionAsync();
      return Results.Ok();

    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      return Results.BadRequest(ex.Message);
    }

  }
}