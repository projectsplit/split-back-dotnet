using CSharpFunctionalExtensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Helper;
public static class IdToName {

  public static async Task<Result<string>> MemberIdToMemberName(string groupId, string memberId, IGroupRepository groupRepository, IUserRepository userRepository) {

    var groupResult = await groupRepository.GetById(groupId);
    if(groupResult.IsFailure) return Result.Failure<string>(groupResult.Error);
    var group = groupResult.Value;

    var members = group.Members.ToList();
    var member = members.First(m => m.MemberId == memberId);

    if(member is GuestMember) {
      var guestMember = (GuestMember)member;
      return guestMember.Name;
    }

    var userMember = (UserMember)member;
    var userMemberResult = await userRepository.GetById(userMember.UserId);
    if(userMemberResult.IsFailure) return Result.Failure<string>(userMemberResult.Error);
    var userFound = userMemberResult.Value;

    return userFound.Nickname;
  }
}