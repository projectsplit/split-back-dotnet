using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Helper;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;
public static partial class InvitationEndpoints {
  private static async Task<IResult> Verify(HttpContext httpContext, IRepository repo, VerifyInvitationDto dto, IOptions<AppSettings> appSettings) {

    var authedUserId = httpContext.GetAuthorizedUserId();
    var client = new MongoClient(appSettings.Value.MongoDb.ConnectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      var invitation = repo.GetInvitationByCode(dto.Code).Result;
      if(invitation is null) return Results.BadRequest("Invitation not found");

      var groupDoc = repo.CheckIfUserInGroupMembers(authedUserId, invitation.GroupId).Result;
      if(groupDoc is null) return Results.BadRequest("Member already exists in group");

      var inviter = repo.GetUserById(invitation.Inviter).Result;
      var userDoc = repo.CheckIfGroupInUser(authedUserId, invitation.GroupId).Result;
      if(userDoc is null) return Results.BadRequest("Group already exists in user");

      await session.CommitTransactionAsync();
      return Results.Ok(new {
        Message = "Invitation is valid",
        InviterNickName = inviter.Nickname,
        group = groupDoc.Title
      });
    } catch(Exception ex) {
      //await session.AbortTransactionAsync();
      return Results.BadRequest(ex.Message);
    }

  }
}