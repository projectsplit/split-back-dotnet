using SplitBackApi.Data;
using SplitBackApi.Requests;
using AutoMapper;

namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {

  private static async Task<IResult> RemoveGuest(
    IRepository repo,
    IMapper mapper,
    RemoveGuestDto removeGuestDto) {

    var addGuestResult = await repo.RemoveGuestFromGroup(removeGuestDto.GroupId, removeGuestDto.UserId);

    return Results.Ok();
  }
}