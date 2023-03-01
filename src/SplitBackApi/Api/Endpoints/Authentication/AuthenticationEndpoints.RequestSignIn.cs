using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RequestSignIn(
    HttpResponse response,
    IUserRepository userRepository,
    AuthService authService,
    RequestSignInRequest request
  ) {

    var userResult = await userRepository.GetByEmail(request.Email);
    if(userResult.IsFailure) return Results.Unauthorized();

    var newUnique = Guid.NewGuid().ToString();

    string token = authService.GenerateSignInRequestToken(
      newUnique,
      request.Email
    );

    response.AppendUniqueCookie(newUnique);

    return Results.Ok(token);
  }
}
