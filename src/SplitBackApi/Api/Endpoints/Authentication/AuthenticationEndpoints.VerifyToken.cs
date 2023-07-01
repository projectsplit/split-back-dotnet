using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Services;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> VerifyToken(
    HttpResponse response,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    AuthService authService,
    VerifyTokenRequest request
  ) {

    var validatedToken = authService.VerifyToken(request.Token);
    if(validatedToken is null) return Results.BadRequest();

    var typeClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "type");
    if(typeClaim is null) return Results.BadRequest("Type claim is missing");

    var uniqueClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "unique");
    if(uniqueClaim is null) return Results.BadRequest("Unique claim is missing");

    var unique = uniqueClaim.Value;
    if(unique is null) return Results.BadRequest("Unique claim is missing");

    var sessionResult = await sessionRepository.GetByUnique(unique);
    if(sessionResult.IsSuccess) return Results.BadRequest("Unique has already been used");

    var emailClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "email");
    if(emailClaim is null) return Results.BadRequest("Email claim is missing");

    var newRefreshToken = Guid.NewGuid().ToString();

    switch(typeClaim.Value) {

      case "sign-up": {

          var nicknameClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "nickname");
          if(nicknameClaim is null) return Results.BadRequest("Nickname claim is missing");

          var newUser = new User {
            Email = emailClaim.Value,
            Nickname = nicknameClaim.Value,
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow,
          };
          await userRepository.Create(newUser);

          var newSession = new JwtAuthSession {
            RefreshToken = newRefreshToken,
            UserId = newUser.Id,
            Unique = unique,
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow
          };
          await sessionRepository.Create(newSession);

          return Results.Ok(new { type = "sign-up" });
        }

      case "sign-in": {

          var userResult = await userRepository.GetByEmail(emailClaim.Value);
          if(userResult.IsFailure) return Results.BadRequest($"User with email {emailClaim.Value} does not exist");
          var user = userResult.Value;

          var newSession = new JwtAuthSession {
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Unique = unique,
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = DateTime.UtcNow
          };

          await sessionRepository.Create(newSession);

          return Results.Ok(new { type = "sign-in" });
        }

      default:
        return Results.BadRequest("Type claim is not valid");
    }
  }
}
