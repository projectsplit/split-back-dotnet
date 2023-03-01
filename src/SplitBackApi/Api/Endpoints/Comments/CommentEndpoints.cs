namespace SplitBackApi.Endpoints;

public static partial class CommentEndpoints {
  
  public static void MapCommentEndpoints(this IEndpointRouteBuilder app) {
    
    var expenseGroup = app.MapGroup("/comment")
      .WithTags("Comments");
     
    expenseGroup.MapPost("/create", CreateComment);
    expenseGroup.MapPost("/edit", EditComment);
    expenseGroup.MapPost("/delete", DeleteComment);
  }
}