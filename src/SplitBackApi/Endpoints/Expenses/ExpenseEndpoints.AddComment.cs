using SplitBackApi.Data;
using SplitBackApi.Requests;
using SplitBackApi.Extensions;
using AutoMapper;
using SplitBackApi.Domain;
using MongoDB.Bson;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints {

  private static async Task<IResult> AddComment(
   IRepository repo,
   HttpContext httpContext,
   NewCommentDto newCommentDto,
   IMapper mapper,
   RoleService roleService
  ) {
   
    //var endpointPermissionList = new List<Permissions> { Permissions.CanCommentExpense };
    var authedUserId = httpContext.GetAuthorizedUserId();

    try {
      var expenseId = ObjectId.Parse(newCommentDto.ExpenseId);
      var groupId = ObjectId.Parse(newCommentDto.GroupId);

      //var accessAllowed = roleService.PermissionCheck(authedUserId, groupId, 16);

      //if(accessAllowed) {

      var newComment = mapper.Map<Comment>(newCommentDto);
      newComment.CommentorId = authedUserId;

      var addCommentResult = await repo.AddComment(newComment, expenseId, groupId);
      if(addCommentResult.IsFailure) return Results.BadRequest(addCommentResult.Error);

      return Results.Ok();
      //}

      //return Results.BadRequest("Access denied");

    } catch(Exception ex) {
      return Results.BadRequest(ex.Message);
    }
  }
}