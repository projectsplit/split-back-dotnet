using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Comments.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.CommentRepository;

namespace SplitBackApi.Api.Endpoints.Comments;

public static partial class CommentEndpoints {

  private static async Task<IResult> DeleteComment(
   ICommentRepository commentRepository,
   ClaimsPrincipal claimsPrincipal,
   DeleteCommentRequest request
  ) {

    var authenticatedUserIdResult = claimsPrincipal.GetAuthenticatedUserId();

    var currentCommentResult = await commentRepository.GetById(request.CommentId);
    if(currentCommentResult.IsFailure) Results.BadRequest(currentCommentResult.Error);
    var currentComment = currentCommentResult.Value;

    //TODO validate edited comment

    var updateResult = await commentRepository.DeleteById(request.CommentId);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to delete comment");

    return Results.Ok();
  }
}