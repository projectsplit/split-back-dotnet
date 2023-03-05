namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> GoogleUrl(
    HttpContext httpContext,
    HttpRequest httpRequest
  ) {   
    // Construct Url for Sign In With Google Button
    
    // Send Url
    
    return Results.Ok();
  }
}
