using System.Security.Claims;
using MongoDB.Bson.Serialization.Attributes;
using SplitBackApi.Api.Endpoints.Groups.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;


// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetGroupById(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    GetGroupByIdRequest request

  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var membersWithNamesResult = await MemberIdToNameHelper.MembersWithNames(group, userRepository);
    if(membersWithNamesResult.IsFailure) return Results.BadRequest(membersWithNamesResult.Error);
    var membersWithNames = membersWithNamesResult.Value;

    var members = group.Members.Select(m => new GroupMemberWithNameAndType {
      MemberId = m.MemberId,
      UserId = (m is UserMember) ? ((UserMember)m).UserId : "",
      Permissions = m.Permissions,
      Name = membersWithNames.Single(mn => mn.Id == m.MemberId).Name,
      MemberType = m.GetType()
        .GetCustomAttributes(typeof(BsonDiscriminatorAttribute), true)
        .Cast<BsonDiscriminatorAttribute>()
        .FirstOrDefault().Discriminator ?? ""

    }).ToList();

    var response = new GroupResponse {
      Members = members,
      BaseCurrency = group.BaseCurrency,
      CreationTime = group.CreationTime,
      Id = group.Id,
      Labels = group.Labels,
      LastUpdateTime = group.LastUpdateTime,
      OwnerId = group.OwnerId,
      Title = group.Title
    };

    return Results.Ok(response);
  }
}