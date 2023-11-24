using CSharpFunctionalExtensions;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Helper;
public static class MemberIdHelper
{
  public static string UserIdToMemberId(Group group, string userId)
  {
    var userMembers = group.Members.Where(m => m is UserMember).Cast<UserMember>();
    var memberId = userMembers.Single(um => um.UserId == userId).MemberId;

    return memberId;
  }

  public static Dictionary<string, string> GroupIdsToMemberIdsMap(IEnumerable<Group> groups, string userId)
  {
    return groups.ToDictionary(
         group => group.Id,
         group => UserIdToMemberId(group, userId));
  }
}