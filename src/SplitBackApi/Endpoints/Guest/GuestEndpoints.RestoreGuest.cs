using SplitBackApi.Data;
using SplitBackApi.Requests;
using AutoMapper;
using MongoDB.Bson;

namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {

  private static async Task<IResult> RestoreGuest(
    IRepository repo,
    IMapper mapper,
    RestoreGuestDto restoreGuestDto) {

    var groupId = ObjectId.Parse(restoreGuestDto.GroupId);
    var userId = ObjectId.Parse(restoreGuestDto.UserId);

    var addGuestResult = await repo.RestoreGuestToGroup(groupId, userId);

    return Results.Ok();
  }
}