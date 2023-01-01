using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> SignIn(
    HttpResponse response,
    IRepository repo,
    AuthService authService,
    HttpRequest request
  ) {
    
    var unique = request.Cookies["unique"];
    if(unique is null) return Results.Unauthorized();
    
    var sessionFound = await repo.GetSessionByUnique(unique);
    if(sessionFound is null) return Results.Unauthorized();
    
    var userFound = await repo.GetUserById(sessionFound.UserId);
    if(userFound is null) return Results.Unauthorized();
    
    response.DeleteUniqueCookie();
    response.AppendRefreshTokenCookie(sessionFound.RefreshToken);
    
    var accessToken = authService.GenerateAccessToken(userFound.Id.ToString());
    
    return Results.Ok(accessToken);
  }
}
