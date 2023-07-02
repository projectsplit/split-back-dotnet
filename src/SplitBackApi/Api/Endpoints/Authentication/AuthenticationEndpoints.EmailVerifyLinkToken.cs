using CSharpFunctionalExtensions;
using MongoDB.Bson;
using SplitBackApi.Api.Endpoints.Authentication.Models;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Endpoints.Authentication.Responses;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints
{
  public static async Task<Microsoft.AspNetCore.Http.IResult> EmailVerifyLinkToken(
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    AuthService authService,
    HttpRequest request)
  {
    var token = request.Query["token"].ToString();
    if (string.IsNullOrEmpty(token)) return Results.BadRequest("Token is missing");

    var emailLinkTokenClaimsResult = await ParseEmailLinkToken(
      token,
      sessionRepository,
      authService
    );

    if (emailLinkTokenClaimsResult.IsFailure) return Results.BadRequest(emailLinkTokenClaimsResult.Error);
    var emailLinkTokenClaims = emailLinkTokenClaimsResult.Value;

    var userResult = await GetOrCreateUser(
      emailLinkTokenClaims.IsNewUser,
      emailLinkTokenClaims.Email,
      userRepository
    );

    if (userResult.IsFailure) return Results.BadRequest(userResult.Error);
    var user = userResult.Value;

    var newRefreshToken = Guid.NewGuid().ToString();

    var newSession = new JwtAuthSession
    {
      RefreshToken = newRefreshToken,
      UserId = user.Id,
      Unique = emailLinkTokenClaims.Unique,
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow
    };

    await sessionRepository.Create(newSession);

    return Results.Ok(new VerifyEmailLinkTokenResponse(emailLinkTokenClaims.IsNewUser));
  }

  private static async Task<Result<User>> GetOrCreateUser(
    bool isNewUser,
    string email,
    IUserRepository userRepository)
  {
    if (isNewUser)
    {
      var nickname = email.Split("@").FirstOrDefault();
      if (nickname is null) return Result.Failure<User>("Unable to parse email for nickname");

      var newUser = new User
      {
        Id = ObjectId.GenerateNewId().ToString(),
        Email = email,
        Nickname = nickname,
        CreationTime = DateTime.UtcNow,
        LastUpdateTime = DateTime.UtcNow,
      };

      await userRepository.Create(newUser);
      return newUser;
    }

    var userResult = await userRepository.GetByEmail(email);
    if (userResult.IsFailure) return Result.Failure<User>($"User with email {email} does not exist");

    return userResult.Value;
  }

  private static async Task<Result<EmailLinkTokenClaims>> ParseEmailLinkToken(
    string token,
    ISessionRepository sessionRepository,
    AuthService authService)
  {
    var validatedTokenResult = authService.VerifyToken(token);
    if (validatedTokenResult.IsFailure) return Result.Failure<EmailLinkTokenClaims>(validatedTokenResult.Error);
    var validatedToken = validatedTokenResult.Value;
    
    if (validatedToken is null) return Result.Failure<EmailLinkTokenClaims>("Token is not valid");

    var newUserClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "new-user");
    if (newUserClaim is null) return Result.Failure<EmailLinkTokenClaims>("new-user claim is missing");
    if (bool.TryParse(newUserClaim.Value, out var isNewUser) is false) return Result.Failure<EmailLinkTokenClaims>("new-user claim is invalid");

    var uniqueClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "unique");
    if (uniqueClaim is null) return Result.Failure<EmailLinkTokenClaims>("unique claim is missing");

    var unique = uniqueClaim.Value;
    if (unique is null) return Result.Failure<EmailLinkTokenClaims>("Unique claim is missing");

    var sessionResult = await sessionRepository.GetByUnique(unique);
    if (sessionResult.IsSuccess) return Result.Failure<EmailLinkTokenClaims>("Unique has already been used");

    var emailClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "email");
    if (emailClaim is null) return Result.Failure<EmailLinkTokenClaims>("Email claim is missing");
    var email = emailClaim.Value;

    return new EmailLinkTokenClaims(email, unique, isNewUser);
  }
}
