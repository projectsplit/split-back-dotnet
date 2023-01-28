using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;

using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {
  
  private static async Task<IResult> Create(
    IRepository repo,
    InvitationDto invitationDto,
    IOptions<AppSettings> appSettings) {

    var client = new MongoClient(appSettings.Value.MongoDb.ConnectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      var inviterID = ObjectId.Parse(invitationDto.InviterId);
      var groupID = ObjectId.Parse(invitationDto.GroupId);
      var invitationFound = await repo.GetInvitationByInviter(inviterID, groupID);
      if(invitationFound is null) {
        await repo.CreateInvitation(inviterID, groupID);
        await session.CommitTransactionAsync();
        return Results.Ok();
      } else {
        await session.CommitTransactionAsync();
        return Results.BadRequest($"Invitation for group with id {groupID} already exists");
      }

    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      return Results.BadRequest(ex.Message);
    }
  }
}