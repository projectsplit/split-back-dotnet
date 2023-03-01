using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Domain;
using System.Security.Claims;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class CommentEndpoints {

  private static async Task<IResult> EditComment(
   ICommentRepository commentRepository,
   IGroupRepository groupRepository,
   ClaimsPrincipal claimsPrincipal,
   EditCommentRequest request
  ) {
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var currentCommentResult = await commentRepository.GetById(request.CommentId);
    if(currentCommentResult.IsFailure) Results.BadRequest(currentCommentResult.Error);
    var currentComment = currentCommentResult.Value;
    
    // TODO Forbid if permissions are missing

    var editedComment = new Comment {
      Id = currentComment.Id,
      CreationTime = currentComment.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      MemberId = currentComment.MemberId,
      ParentId = currentComment.ParentId,
      Text = request.Text
    };

    //TODO validate edited comment

    var updateResult = await commentRepository.Update(editedComment);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to update comment");

    return Results.Ok();
  }
}