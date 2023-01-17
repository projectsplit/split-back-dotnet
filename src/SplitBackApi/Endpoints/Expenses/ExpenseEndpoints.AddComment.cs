using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
using AutoMapper;
using SplitBackApi.Domain;
using MongoDB.Bson;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {
  private static async Task<IResult> AddComment(IRepository repo, HttpContext httpContext, NewCommentDto newCommentDto, IMapper mapper) {
    var authedUserId = httpContext.GetAuthorizedUserId();
    try {

      var newComment = mapper.Map<Comment>(newCommentDto);
      newComment.CommentorId = authedUserId;
      var expenseId = ObjectId.Parse(newCommentDto.ExpenseId);
      var groupId = ObjectId.Parse(newCommentDto.GroupId);

      await repo.AddComment(newComment, expenseId, groupId);

      return Results.Ok();
    } catch(Exception ex) {
      return Results.BadRequest(ex.Message);
    }
  }
}