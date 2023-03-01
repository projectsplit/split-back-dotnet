using Microsoft.AspNetCore.Mvc;
using SplitBackApi.Services;
using SplitBackApi.Data;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RequestSignUp(
    HttpResponse response,
    IUserRepository userRepository,
    AuthService authService,
    RequestSignUpRequest request
  ) {
    
    var userResult = await userRepository.GetByEmail(request.Email);
    if(userResult.IsSuccess) return Results.BadRequest("User already exists");

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