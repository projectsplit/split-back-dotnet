namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> GoogleContinue(
    HttpContext httpContext,
    HttpRequest httpRequest
  ) {   
    // Get authorization_code from request
    
    // With this request access token from google
    
    // Get user info with access token
    
    // Get email from info
    
    // if user with email not exists
    
        // Create User with email
        // Create a Session for new User
        // Return refresh & access token
        
    // if user already exists
    
        // Create a Session for new User
        // Return refresh & access token
    
    return Results.Ok();
  }
}
