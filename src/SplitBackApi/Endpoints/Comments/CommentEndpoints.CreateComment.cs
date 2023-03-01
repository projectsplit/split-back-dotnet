using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using SplitBackApi.Domain;
using SplitBackApi.Domain.Extensions;
using System.Security.Claims;

namespace SplitBackApi.Endpoints;

public static partial class CommentEndpoints {

  private static async Task<IResult> CreateComment(
    IGroupRepository groupRepository,
    ICommentRepository commentRepository,
    ClaimsPrincipal claimsPrincipal,
    CreateCommentRequest request
  ) {
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"{authenticatedUserId} is not a member of group with id {request.GroupId}");
    
    var permissions = member.Permissions;
    if(permissions.HasFlag(Permissions.Comment) is false) return Results.Forbid();
    
    var newComment = new Comment {
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
      MemberId = member.MemberId,
      ParentId = request.ParentId,
      Text = request.Text
    };
    
    //TODO validate comment
    
    await commentRepository.Create(newComment);

    return Results.Ok();
  }
}