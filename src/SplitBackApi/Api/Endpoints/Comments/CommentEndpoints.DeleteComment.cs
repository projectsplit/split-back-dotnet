using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Comments.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.CommentRepository;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;

namespace SplitBackApi.Api.Endpoints.Comments;

public static partial class CommentEndpoints {

  private static async Task<IResult> DeleteComment(
   IGroupRepository groupRepository,
   ICommentRepository commentRepository,
   ClaimsPrincipal claimsPrincipal,
   DeleteCommentRequest request
  ) {

    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();

    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");

    if(member.Permissions.HasFlag(Domain.Models.Permissions.Comment) is false) return Results.Forbid();

    var currentCommentResult = await commentRepository.GetById(request.CommentId);
    if(currentCommentResult.IsFailure) return Results.BadRequest(currentCommentResult.Error);
    var currentComment = currentCommentResult.Value;

    if(currentComment.MemberId != member.MemberId && member.Permissions.HasFlag(Domain.Models.Permissions.ManageGroup) is false)
    return Results.Forbid();

    //TODO validate edited comment

    var updateResult = await commentRepository.DeleteById(request.CommentId);
    if(updateResult.IsFailure) return Results.BadRequest("Failed to delete comment");

    return Results.Ok();
  }
}