using CSharpFunctionalExtensions;
using SplitBackApi.Domain;

namespace SplitBackApi.Data;

public partial class MongoDbRepository : IRepository {

  public async Task<Result<Group>> GetGroupIfUserAndGuestCriteriaAreSatisified(string userId, Invitation invitation) {

    switch(invitation) {

      case GuestInvitation: {
          var groupResult = await GetGroupIfUserIsNotMemberAndGuestIsMember(userId, invitation);
          if(groupResult.IsFailure) return Result.Failure<Group>(groupResult.Error);
          return groupResult.Value;
        }

      case UserInvitation: {
          var groupResult = await GetGroupIfUserIsNotMember(userId, invitation);
          if(groupResult.IsFailure) return Result.Failure<Group>(groupResult.Error);
          return groupResult.Value;
        }
      default:
        return Result.Failure<Group>("Not a valid invitation");
    }
  }
}
