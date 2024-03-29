using SplitBackApi.Api.Services.GoogleAuthService;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints
{
  public static IResult GoogleUrl(
    GoogleAuthService googleAuthService)
  {
    var signInWithGoogleUrl = googleAuthService.GenerateSignInWithGoogleUrl();

    return Results.Ok(signInWithGoogleUrl);
  }
}
