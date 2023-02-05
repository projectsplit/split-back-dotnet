using SplitBackApi.Data;
using SplitBackApi.Requests;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {

  private static async Task<IResult> RestoreGuest(
    IRepository repo,
    IMapper mapper,
    RestoreGuestDto restoreGuestDto) {

    var addGuestResult = await repo.RestoreGuestToGroup(restoreGuestDto.GroupId, restoreGuestDto.UserId);

    return Results.Ok();
  }
}