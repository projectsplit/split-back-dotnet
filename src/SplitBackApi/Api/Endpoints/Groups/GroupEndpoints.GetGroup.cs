using System.Security.Claims;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints
{
  private static async Task<IResult> GetGroup(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    HttpRequest request
  )
  {
    var groupId = request.Query["groupId"].ToString();
    if (string.IsNullOrEmpty(groupId)) return Results.BadRequest("Group Id is missing");

    var groupMaybe = await groupRepository.GetById(groupId);

    if (groupMaybe.HasNoValue) return Results.BadRequest("Group not found");

    var group = groupMaybe.Value;

    var userMembers = group.Members
      .Where(m => m is UserMember)
      .Select(m => m as UserMember);

    var guestMembers = group.Members
      .Where(m => m is GuestMember)
      .Select(m => m as GuestMember);

    var userIds =
      userMembers
      .Select(m => m.UserId)
      .ToList();

    var users = await userRepository.GetByIds(userIds);

    var response = new
    {
      group.Id,
      group.Title,
      Currencty = group.BaseCurrency,
      group.LastUpdateTime,
      group.CreationTime,
      group.Labels,
      group.OwnerId,
      members = new
      {
        Users = userMembers.Select(m => new
        {
          m.MemberId,
          Name = users.First(u => m.UserId == u.Id).Nickname
        }),
        Guests = guestMembers
      }
    };

    return Results.Ok(response);
  }
}