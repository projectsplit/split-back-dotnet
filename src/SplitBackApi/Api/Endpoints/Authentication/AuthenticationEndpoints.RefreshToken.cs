using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.SessionRepository;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RefreshToken(
    ISessionRepository sessionRepository,
    AuthService authService,
    HttpRequest request
  ) {

    var refreshToken = request.Cookies["refresh-token"];
    if(refreshToken is null) return Results.BadRequest();

    var sessionResult = await sessionRepository.GetByRefreshToken(refreshToken);
    if(sessionResult.IsFailure) return Results.BadRequest();
    var session = sessionResult.Value;

    var accessToken = authService.GenerateAccessToken(session.UserId.ToString());

    return Results.Ok(new { newAccessToken = accessToken });
  }
}
