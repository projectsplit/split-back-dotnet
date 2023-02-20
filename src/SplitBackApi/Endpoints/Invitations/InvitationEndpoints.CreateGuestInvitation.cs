using SplitBackApi.Data;
using SplitBackApi.Requests;

using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> CreateGuestInvitation(
    IRepository repo,
    GuestInvitationDto guestInvitationDto,
    IOptions<AppSettings> appSettings) {

    var guestInvitationFound = await repo.GetGuestInvitationByInviterIdAndGuestId(guestInvitationDto.InviterId, guestInvitationDto.GroupId, guestInvitationDto.GuestId);

    if(guestInvitationFound is null) {

      await repo.CreateGuestInvitation(guestInvitationDto.InviterId, guestInvitationDto.GroupId, guestInvitationDto.GuestId);

      return Results.Ok();

    } else {

      return Results.BadRequest($"Invitation for guest with id {guestInvitationDto.GuestId} already exists");
    }
  }
}