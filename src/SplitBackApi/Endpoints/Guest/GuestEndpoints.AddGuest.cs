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

    var email = addGuestDto.Email;
    var nickname = addGuestDto.Nickname;
    var addGuestResult = await repo.AddGuestToGroup(addGuestDto.GroupId, email, nickname);

    return Results.Ok();
  }
}