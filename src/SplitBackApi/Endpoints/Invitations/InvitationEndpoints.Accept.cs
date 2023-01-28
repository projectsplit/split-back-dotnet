using SplitBackApi.Data;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Accept(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings
  ) {

    var authenticatedUserId = httpContext.GetAuthorizedUserId();

    var getInvitationResult = await repo.GetInvitationByCode(request.Code);
    if(getInvitationResult.IsFailure) return Results.BadRequest(getInvitationResult.Error);
    var invitation = getInvitationResult.Value;

    var groupDoc = await repo.AddUserInGroupMembersOrFail(authenticatedUserId, invitation.GroupId);
    if(groupDoc.IsFailure) return Results.BadRequest(groupDoc.Error);

    var getUserResult = await repo.GetUserById(invitation.Inviter);
    if(getUserResult.IsFailure) return Results.BadRequest(getUserResult.Error);

    var addGroupInUserResult = await repo.AddGroupInUserOrFail(authenticatedUserId, invitation.GroupId);
    if(addGroupInUserResult.IsFailure) return Results.BadRequest(addGroupInUserResult.Error);

    return Results.Ok(new {
      Message = "User joined group",
      group = groupDoc.Value.Title
    });
  }
}