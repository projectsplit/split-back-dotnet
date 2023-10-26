using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Validators;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints
{
  private static async Task<IResult> EmailSendLink(
    HttpResponse response,
    IUserRepository userRepository,
    AuthService authService,
    EmailInitiateRequest request,
    IOptions<AppSettings> appSettings,
    EmailInitiateValidator emailInitiateValidator)
  {
    var validationResult = emailInitiateValidator.Validate(request);
    if (validationResult.IsValid is false) return Results.BadRequest(validationResult.ToErrorResponse());

    var userResult = await userRepository.GetByEmail(request.Email);

    var isNewUser = userResult.IsFailure;

    var newUnique = Guid.NewGuid().ToString();

    string emailLink = authService.GenerateEmailLink(
      newUnique,
      request.Email,
      isNewUser
    );

    response.AppendUniqueCookie(newUnique);

    Console.WriteLine(emailLink);

    return Results.Ok(new { isNewUser });
  }
}