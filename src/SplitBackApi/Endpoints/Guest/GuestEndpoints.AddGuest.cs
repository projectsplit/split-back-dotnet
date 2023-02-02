using SplitBackApi.Data;
using SplitBackApi.Requests;
using AutoMapper;
using MongoDB.Bson;

namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {

  private static async Task<IResult> AddGuest(
    IRepository repo,
    IMapper mapper,
    AddGuestDto addGuestDto) {

    var groupId = ObjectId.Parse(addGuestDto.GroupId);
    var email = addGuestDto.Email;
    var nickname = addGuestDto.Nickname;
    var addGuestResult = await repo.AddGuestToGroup(groupId, email, nickname);

    return Results.Ok();
  }
}