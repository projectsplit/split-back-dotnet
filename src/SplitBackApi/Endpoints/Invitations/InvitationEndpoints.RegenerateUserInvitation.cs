using SplitBackApi.Data;
using MongoDB.Bson;
using SplitBackApi.Requests;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;
using MongoDB.Driver;

namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> RegenerateUserInvitation(
    IRepository repo,
    InvitationDto invitationDto,
    IOptions<AppSettings> appSettings
  ) {

    var regenerateResult = await repo.RegenerateUserInvitation(invitationDto.InviterId, invitationDto.GroupId);
    if(regenerateResult.IsFailure) return Results.BadRequest(regenerateResult.Error);

    return Results.Ok();


  }
}