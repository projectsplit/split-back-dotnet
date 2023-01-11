using Microsoft.AspNetCore.Mvc;
using SplitBackApi.Data;
using SplitBackApi.Domain;
using SplitBackApi.Services;

namespace SplitBackApi.Endpoints;

public static partial class AuthenticationEndpoints {

  public record VerifyTokenRequest(string Token);

  public static async Task<IResult> VerifyToken(
    HttpResponse response,
    IRepository repo,
    AuthService authService,
    [FromBody] VerifyTokenRequest request
  ) {

    var validatedToken = authService.VerifyToken(request.Token);
    if(validatedToken is null) return Results.BadRequest();

    var typeClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "type");
    if(typeClaim is null) return Results.BadRequest("Type claim is missing");

    var uniqueClaim = validatedToken.Payload.Claims.FirstOrDefault(claim => claim.Type == "unique");
    if(uniqueClaim is null) return Results.BadRequest("Unique claim is missing");

    var unique = uniqueClaim.Value;
    if(unique is null) return Results.BadRequest("Unique claim is missing");

    var sessionFound = await repo.GetSessionByUnique(unique);
    if(sessionFound is not null) return Results.BadRequest("Unique has already been used");

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
          };
          await repo.AddUser(newUser);

          var newSession = new Session {
            RefreshToken = newRefreshToken,
            UserId = newUser.Id,
            Unique = unique
          };
          await repo.AddSession(newSession);

          return Results.Ok();
        }

      case "sign-in": {

          var userFound = await repo.GetUserByEmail(emailClaim.Value);
          if(userFound is null) return Results.NotFound("User does not exist");

          await repo.AddSession(new Session {
            RefreshToken = newRefreshToken,
            UserId = userFound.Id,
            Unique = unique
          });

          return Results.Ok();
        }

      default:
        return Results.BadRequest("Type claim is not valid");
    }
  }
}
