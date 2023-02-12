using SplitBackApi.Data;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Extensions;
using SplitBackApi.Helper;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> Accept(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings
  ) {

    var authenticatedUserIdResult = httpContext.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var getInvitationResult = await repo.GetInvitationByCode(request.Code);
    if(getInvitationResult.IsFailure) return Results.BadRequest(getInvitationResult.Error);
    var invitation = getInvitationResult.Value;

    var getUserResult = await repo.GetUserById(invitation.Inviter);
    if(getUserResult.IsFailure) return Results.BadRequest(getUserResult.Error);

    var processInvitationResult = await repo.ProcessInvitation(authenticatedUserId, invitation);
    if(processInvitationResult.IsFailure) return Results.BadRequest(processInvitationResult.Error);

    return Results.Ok(new {
      Message = $"User {authenticatedUserId} joined group",

    });
  }
}