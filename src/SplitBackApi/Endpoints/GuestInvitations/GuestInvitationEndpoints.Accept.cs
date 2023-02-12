using SplitBackApi.Data;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class GuestInvitationEndpoints {

  private static async Task<IResult> Accept(
    HttpContext httpContext,
    IRepository repo,
    VerifyInvitationRequest request,
    IOptions<AppSettings> appSettings
  ) {

    var authenticatedUserIdResult = httpContext.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var getGuestInvitationResult = await repo.GetGuestInvitationByCode(request.Code);
    if(getGuestInvitationResult.IsFailure) return Results.BadRequest(getGuestInvitationResult.Error);
    var guestInvitation = getGuestInvitationResult.Value;

    var getUserResult = await repo.GetUserById(guestInvitation.Inviter);
    if(getUserResult.IsFailure) return Results.BadRequest(getUserResult.Error);

    await repo.ProcessInvitation(authenticatedUserId, guestInvitation);

    return Results.Ok( $"Guest was replaced by user {authenticatedUserId} successfully");
  }
}