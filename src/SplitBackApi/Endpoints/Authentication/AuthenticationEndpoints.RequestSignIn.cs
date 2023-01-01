using Microsoft.AspNetCore.Mvc;
using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private record RequestSignInRequest(string Email);

  private static async Task<IResult> RequestSignIn(
    HttpResponse response,
    IRepository repo,
    AuthService authService,
    [FromBody] RequestSignInRequest request
  ) {

    var userFound = await repo.GetUserByEmail(request.Email);
    if(userFound is null) return Results.Unauthorized();

    var newUnique = Guid.NewGuid().ToString();

    string token = authService.GenerateSignInRequestToken(
      newUnique,
      request.Email
    );

    response.AppendUniqueCookie(newUnique);

    return Results.Ok(token);
  }
}
