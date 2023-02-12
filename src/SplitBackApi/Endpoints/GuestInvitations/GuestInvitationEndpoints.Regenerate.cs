using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;

public static partial class GuestInvitationEndpoints {

  private static async Task<IResult> Regenerate(
    IRepository repo,
    GuestInvitationDto guestInvitationDto,
    IOptions<AppSettings> appSettings
  ) {

    var regenerateResult = await repo.RegenerateGuestInvitation(guestInvitationDto.InviterId, guestInvitationDto.GroupId, guestInvitationDto.GuestId);
    if(regenerateResult.IsFailure) return Results.BadRequest(regenerateResult.Error);

    return Results.Ok();

  }
}