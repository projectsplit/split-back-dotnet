using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Groups.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<IResult> CreateGroup(
    GroupValidator groupValidator,
    ClaimsPrincipal claimsPrincipal,
    IGroupRepository groupRepository,
    CreateGroupRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var newGroup = new Group {
      BaseCurrency = request.BaseCurrencyCode,
      OwnerId = authenticatedUserId,
      Title = request.Title,
      Labels = request.Labels.Select(l => new Label {
        Id = Guid.NewGuid().ToString(),
        Color = l.Color,
        Text = l.Text
      }).ToList(),
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow
    };

    var allPermissions = Enum
      .GetValues(typeof(Domain.Models.Permissions))
      .Cast<Domain.Models.Permissions>()
      .Aggregate((current, next) => current | next);
      
    newGroup.Members.Add(new UserMember {
      MemberId = Guid.NewGuid().ToString(),
      UserId = authenticatedUserId,
      Permissions = allPermissions
    });
    
    var validationResult = groupValidator.Validate(newGroup);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    await groupRepository.Create(newGroup);
    
    return Results.Ok();
  }
}