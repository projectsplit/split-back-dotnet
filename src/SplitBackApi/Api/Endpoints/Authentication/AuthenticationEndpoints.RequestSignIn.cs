using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RequestSignIn(
    HttpResponse response,
    IUserRepository userRepository,
    AuthService authService,
    RequestSignInRequest request,
    IOptions<AppSettings> appSettings,
    SignInValidator signInValidator
  ) {

    var validationResult = signInValidator.Validate(request);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());
    
    var userResult = await userRepository.GetByEmail(request.Email);
    if(userResult.IsFailure) return Results.Unauthorized();

    var newUnique = Guid.NewGuid().ToString();

    string token = authService.GenerateSignInRequestToken(
      newUnique,
      request.Email
    );

    response.AppendUniqueCookie(newUnique);
    
    Console.WriteLine($"{appSettings.Value.FrontendUrl}/s/{token}");
    return Results.Ok(token);
  }
  
}
