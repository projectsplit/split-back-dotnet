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

  private static async Task<IResult> EditComment(
   ICommentRepository commentRepository,
   IGroupRepository groupRepository,
   ClaimsPrincipal claimsPrincipal,
   CommentValidator commentValidator,
   EditCommentRequest request
  ) {

    var groupMaybe = await groupRepository.GetById(request.GroupId);
    if(groupMaybe.HasNoValue) return Results.BadRequest("Group not found");
    var group = groupMaybe.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.Comment) is false) return Results.Forbid();

    var currentCommentResult = await commentRepository.GetById(request.CommentId);

    if(currentCommentResult.IsFailure) return Results.BadRequest(currentCommentResult.Error);
    var currentComment = currentCommentResult.Value;

    if(currentComment.MemberId != member.MemberId) return Results.Forbid();

    var editedComment = new Comment {
      Id = currentComment.Id,
      CreationTime = currentComment.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      MemberId = currentComment.MemberId,
      ParentId = currentComment.ParentId,
      Text = request.Text
    };

    var validationResult = commentValidator.Validate(editedComment);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var updateResult = await commentRepository.Update(editedComment);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update comment");

    return Results.Ok();
  }
}