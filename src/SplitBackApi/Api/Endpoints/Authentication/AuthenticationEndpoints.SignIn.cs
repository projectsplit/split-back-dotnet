using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;

namespace SplitBackApi.Api.Endpoints.Authentication;

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
    var sessionData = new SessionData {
      Id = session.Id,
      UserId = session.UserId,
      UserEmail = user.Email,
      UserNickname = user.Nickname
    };

    return Results.Ok(new {
      accessToken = accessToken,
      sessionData = sessionData
    });
  }
}
