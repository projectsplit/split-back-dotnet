using Microsoft.AspNetCore.Mvc;
using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private record RequestSignUpRequest(string Email, string Nickname);

  private static async Task<IResult> RequestSignUp(
    HttpResponse response,
    IRepository repo,
    AuthService authService,
    [FromBody] RequestSignUpRequest request
  ) {

    if(await repo.UserExistsByEmail(request.Email)) {
      return Results.Ok("User already exists!");
    }

    var newUnique = Guid.NewGuid().ToString();

    string token = authService.GenerateSignUpRequestToken(
      newUnique,
      request.Email,
      request.Nickname
    );

    response.AppendUniqueCookie(newUnique);
    
    return Results.Ok($"{token}");
  }
}