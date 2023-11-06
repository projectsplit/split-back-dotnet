using System.Security.Claims;
using MongoDB.Bson.Serialization.Attributes;
using SplitBackApi.Api.Endpoints.Groups.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Helper;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GuestEndpoints {

  private static async Task<IResult> CreateGuest(
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    GuestMemberValidator guestMemberValidator,
    CreateGuestRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId =claimsPrincipal.GetAuthenticatedUserId();

    var guestCreatorMember = group.GetMemberByUserId(authenticatedUserId);
    if(guestCreatorMember is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(guestCreatorMember.Permissions.HasFlag(Domain.Models.Permissions.WriteAccess) is false) return Results.Forbid();

    var userMemberIds = group.Members.Where(m => m is UserMember).Cast<UserMember>().Select(m => m.UserId).ToList();
    var usersResult = await userRepository.GetByIds(userMemberIds);
    var users = usersResult.Value;
    var userNameExists = users.Any(u => u.Nickname == request.Name);
    if(userNameExists) return Results.BadRequest($"A member with the name {request.Name} already exists");

    var guestMemberNames = group.Members.Where(m => m is GuestMember).Cast<GuestMember>().Select(m => m.Name).ToList();
    var guestNameExists = guestMemberNames.Any(gmn => gmn == request.Name);
    if(guestNameExists) return Results.BadRequest($"A member with the name {request.Name} already exists");

    var newGuest = new GuestMember {
      MemberId = Guid.NewGuid().ToString(),
      Name = request.Name
    };
    
    var valdationResult = guestMemberValidator.Validate(newGuest);
    if(valdationResult.IsValid is false) return Results.BadRequest(valdationResult.ToErrorResponse());
  
    group.Members.Add(newGuest);

    var groupUpdateResult = await groupRepository.Update(group);
    if(groupUpdateResult.IsFailure) return Results.BadRequest(groupUpdateResult.Error);

    //response
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