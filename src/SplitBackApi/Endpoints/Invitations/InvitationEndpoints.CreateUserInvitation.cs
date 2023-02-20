using SplitBackApi.Data;
using SplitBackApi.Requests;

using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;


namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {

  private static async Task<IResult> CreateUserInvitation(
    IRepository repo,
    InvitationDto invitationDto,
    IOptions<AppSettings> appSettings) {

    var invitationFound = await repo.GetInvitationByInviter(invitationDto.InviterId, invitationDto.GroupId);

    if(invitationFound is null) {

      await repo.CreateUserInvitation(invitationDto.InviterId, invitationDto.GroupId);

      return Results.Ok();

    } 

      return Results.BadRequest($"Invitation for group with id {invitationDto.GroupId} already exists");

  }
}