using SplitBackApi.Data;
using SplitBackApi.Requests;
using AutoMapper;
using MongoDB.Bson;

namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {

  private static async Task<IResult> RemoveGuest(
    IRepository repo,
    IMapper mapper,
    RemoveGuestDto removeGuestDto) {

    var groupId = ObjectId.Parse(removeGuestDto.GroupId);
    var userId = ObjectId.Parse(removeGuestDto.UserId);

    var addGuestResult = await repo.RemoveGuestFromGroup(groupId, userId);

    return Results.Ok();
  }
}