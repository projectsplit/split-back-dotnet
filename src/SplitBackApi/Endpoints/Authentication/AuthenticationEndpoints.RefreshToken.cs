using SplitBackApi.Services;
using SplitBackApi.Data;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RefreshToken(
    IRepository repo,
    AuthService authService,
    HttpRequest request
  ) {
    
    var refreshToken = request.Cookies["refresh-token"];
    if(refreshToken is null) return Results.BadRequest();
    
    var sessionFound = await repo.GetSessionByRefreshToken(refreshToken);
    if(sessionFound is null) return Results.BadRequest();
    
    var accessToken = authService.GenerateAccessToken(sessionFound.UserId.ToString());
    
    return Results.Ok(accessToken);
  }
}
