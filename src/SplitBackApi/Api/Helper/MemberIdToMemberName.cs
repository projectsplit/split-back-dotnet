using CSharpFunctionalExtensions;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Helper;
public static class MemberIdToNameHelper {

  public static async Task<Result<IEnumerable<MemberWithName>>> MembersWithNames(Group group, IUserRepository userRepository) {

    var userMembers = group.Members.Where(m => m is UserMember).Cast<UserMember>();
    var guestMembers = group.Members.Where(m => m is GuestMember).Cast<GuestMember>();

    var usersResult = await userRepository.GetByIds(userMembers.Select(um => um.UserId).ToList());
    if(usersResult.IsFailure) return Result.Failure<IEnumerable<MemberWithName>>(usersResult.Error);

    var users = usersResult.Value;

    var membersWithNames = userMembers.Select(m => new MemberWithName {
      Id = m.MemberId,
      Name = users.Single(u => u.Id == m.UserId).Nickname
    }).Concat(guestMembers.Select(m => new MemberWithName {
      Id = m.MemberId,
      Name = m.Name
    }));

    return Result.Success(membersWithNames);
  }
}