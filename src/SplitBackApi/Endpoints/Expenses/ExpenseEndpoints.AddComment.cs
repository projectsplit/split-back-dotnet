using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using AutoMapper;
using SplitBackApi.Domain;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> AddComment(
   IRepository repo,
   HttpContext context,
   NewCommentDto newCommentDto,
   IMapper mapper,
   RoleService roleService
  ) {

    var authenticatedUserIdResult = context.GetAuthorizedUserId();
    if(authenticatedUserIdResult.IsFailure) return Results.BadRequest(authenticatedUserIdResult.Error);
    var authenticatedUserId = authenticatedUserIdResult.Value;

    var newComment = mapper.Map<Comment>(newCommentDto);
    newComment.CommentorId = authenticatedUserId;

    var addCommentResult = await repo.AddComment(newComment, newCommentDto.ExpenseId, newCommentDto.GroupId);
    if(addCommentResult.IsFailure) return Results.BadRequest(addCommentResult.Error);

    return Results.Ok();
  }
}