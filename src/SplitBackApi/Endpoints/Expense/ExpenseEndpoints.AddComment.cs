using SplitBackApi.Data;
using SplitBackApi.Endpoints.Requests;
using SplitBackApi.Extensions;
namespace SplitBackApi.Endpoints;

public static partial class ExpenseEndpoints
{
    private static async Task<IResult> AddComment(IRepository repo, HttpContext httpContext, NewCommentDto newComment)
    {
       var authedUserId = httpContext.GetAuthorizedUserId();
      try
      {
        await repo.AddComment(newComment, authedUserId);
        return Results.Ok();
      }
      catch (Exception ex)
      {
        return Results.BadRequest(ex.Message);
      }
    }
}