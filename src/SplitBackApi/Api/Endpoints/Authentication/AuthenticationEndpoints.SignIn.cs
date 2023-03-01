using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> SignIn(
    HttpResponse response,
    ISessionRepository sessionRepository,
    IUserRepository userRepository,
    AuthService authService,
    HttpRequest request
  ) {

    var unique = request.Cookies["unique"];
    if(unique is null) return Results.Unauthorized();

    var sessionResult = await sessionRepository.GetByUnique(unique);
    if(sessionResult.IsFailure) return Results.Unauthorized();
    var session = sessionResult.Value;

    var userResult = await userRepository.GetById(session.UserId);
    if(userResult.IsFailure) return Results.Unauthorized();
    var user = userResult.Value;

    response.DeleteUniqueCookie();
    response.AppendRefreshTokenCookie(session.RefreshToken);

    var accessToken = authService.GenerateAccessToken(user.Id.ToString());

    return Results.Ok(accessToken);
  }
}
