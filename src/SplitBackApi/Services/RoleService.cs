using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using SplitBackApi.Data;
using SplitBackApi.Domain;

namespace SplitBackApi.Services;

public class RoleService {

  public Result<bool> MemberHasRequiredPermissions(ObjectId userId, Group group, Permissions requiredPermissions) {

    var member = group.Members.Where(member => member.UserId == userId).SingleOrDefault();
    if(member is null) return Result.Failure<bool>($"Member with userId {userId} not found");

    var groupRoles = group.Roles;
    var memberRoleIds = member.Roles;
    var userRoles = groupRoles.Where(groupRole => memberRoleIds.Contains(groupRole.Id));

    var allMemberPermissions = userRoles.Select(role => role.Permissions).Aggregate((current, next) => current | next);

    return allMemberPermissions.HasFlag(requiredPermissions);
  }

  public Role CreateDefaultRole(string title) {

    switch(title) {

      case "Everyone":
        return new Role {
          Title = title,
          Permissions = 
            Permissions.CanInviteMembers |
            Permissions.CanEditLabels |
            Permissions.CanAddExpense |
            Permissions.CanAddExpense |
            Permissions.CanDeleteExpense |
            Permissions.CanCommentExpense |
            Permissions.CanAddSpenderOtherThanHimself |
            Permissions.CanAddTransfer |
            Permissions.CanDeleteTransfer |
            Permissions.CanAddTransferOnBehalfOfOtheruser
        };

      case "Owner":
        return new Role {
          Title = title,
          Permissions = Enum.GetValues(typeof(Permissions)).Cast<Permissions>().Aggregate((current, next) => current | next)
        };

      default:
        throw new DefaultRoleException("Default Role Exception");
    }
  }
}