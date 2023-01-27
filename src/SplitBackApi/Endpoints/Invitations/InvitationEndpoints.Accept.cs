using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;

using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Extensions;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;
public static partial class InvitationEndpoints {
  private static async Task<IResult> Accept(HttpContext httpContext, IRepository repo, VerifyInvitationDto dto, IOptions<AppSettings> appSettings) {

    var authedUserId = httpContext.GetAuthorizedUserId();
    var client = new MongoClient(appSettings.Value.MongoDb.ConnectionString);
    using var session = await client.StartSessionAsync();
    session.StartTransaction();
    try {
      var invitation = repo.GetInvitationByCode(dto.Code).Result;
      if(invitation.IsFailure) return Results.BadRequest(invitation.Error);

      var groupDoc = repo.CheckAndAddUserInGroupMembers(authedUserId, invitation.Value.GroupId).Result;
      if(groupDoc.IsFailure) return Results.BadRequest(groupDoc.Error);

      var inviter = repo.GetUserById(invitation.Value.Inviter).Result;
      if(inviter.IsFailure) return Results.BadRequest(inviter.Error);
      
      var userDoc = repo.CheckAndAddGroupInUser(authedUserId, invitation.Value.GroupId).Result;
      if(userDoc.IsFailure) return Results.BadRequest(userDoc.Error);

      await session.CommitTransactionAsync();
      return Results.Ok(new {
        Message = "User joined group",
        group = groupDoc.Value.Title
      });
    } catch(Exception ex) {
      await session.AbortTransactionAsync();
      return Results.BadRequest(ex.Message);
    }

  }
}