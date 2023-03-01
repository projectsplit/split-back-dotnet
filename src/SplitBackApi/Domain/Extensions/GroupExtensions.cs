namespace SplitBackApi.Domain.Extensions;

public static class GroupExtensions {

  public static UserMember? GetMemberByUserId(this Group group, string userId) {

    return
      group.Members
        .Where(m => m is UserMember)
        .Cast<UserMember>()
        .Where(um => um.UserId == userId)
        .FirstOrDefault();
  }
}