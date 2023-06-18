using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.UserRepository;

namespace SplitBackApi.Api.Endpoints.Authentication;

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

    string emailLinkToken = authService.GenerateSignInRequestToken(
      newUnique,
      request.Email
    );
    
    //TODO send email with emailLinkToken
    
    Console.WriteLine($"http://localhost:3000/s/{emailLinkToken}");

    response.AppendUniqueCookie(newUnique);

    return Results.Ok(authService.GenerateAccessToken(userResult.Value.Id));
  }
}
