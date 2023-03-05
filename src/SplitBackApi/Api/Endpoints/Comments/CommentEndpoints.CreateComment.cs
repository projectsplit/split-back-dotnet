using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Comments.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.CommentRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Models;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Comments;

public static partial class CommentEndpoints {

  private static async Task<IResult> CreateComment(
    IGroupRepository groupRepository,
    ICommentRepository commentRepository,
    ClaimsPrincipal claimsPrincipal,
    CommentValidator commentValidator,
    CreateCommentRequest request
  ) {
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    var permissions = member.Permissions;
    if(permissions.HasFlag(Domain.Models.Permissions.Comment) is false) return Results.Forbid();
    
    var newComment = new Comment {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      MemberId = member.MemberId,
      ParentId = request.ParentId,
      Text = request.Text
    };

    var validationResult = commentValidator.Validate(newComment);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());
    
    await commentRepository.Create(newComment);

    return Results.Ok();
  }
}