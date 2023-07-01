using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  private static async Task<IResult> RequestSignUp(
    HttpResponse response,
    IUserRepository userRepository,
    AuthService authService,
    IOptions<AppSettings> appSettings,
    RequestSignUpRequest request,
    SignUpValidator signUpValidator
  ) {

    var validationResult = signUpValidator.Validate(request);
    if(validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var userResult = await userRepository.GetByEmail(request.Email);
    if(userResult.IsSuccess) return Results.BadRequest("User already exists");

    var newUnique = Guid.NewGuid().ToString();

    string token = authService.GenerateSignUpRequestToken(
      newUnique,
      request.Email,
      request.Nickname
    );

    response.AppendUniqueCookie(newUnique);

    Console.WriteLine($"{appSettings.Value.FrontendUrl}/s/{token}");
    return Results.Ok(token);
  }
}