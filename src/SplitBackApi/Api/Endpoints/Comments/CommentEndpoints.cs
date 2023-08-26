namespace SplitBackApi.Api.Endpoints.Comments;

public static partial class CommentEndpoints {
  
  public static void MapCommentEndpoints(this IEndpointRouteBuilder app) {
    
    var commentGroup = app.MapGroup("/comment")
      .WithTags("Comments");
     
    commentGroup.MapPost("/create", CreateComment);
    commentGroup.MapPost("/edit", EditComment);
    commentGroup.MapPost("/delete", DeleteComment);
  }
}