using System.Net.Http.Headers;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GoogleUserRepository;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> ContinueWithGoogle(
    HttpResponse response,
    ContinueWithGoogleRequest request,
    IOptions<AppSettings> appSettings,
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ISessionRepository sessionRepository,
    AuthService authService
  ) {

    var clientId = appSettings.Value.Google.ClientId;
    var clientSecret = appSettings.Value.Google.ClientSecret;

    var queryString = request.RedirectUrl;
    var code = HttpUtility.ParseQueryString(queryString).Get("code");
    var scopee = HttpUtility.ParseQueryString(queryString).Get("scope");
    var authuser = HttpUtility.ParseQueryString(queryString).Get("authUser");
    var prompt = HttpUtility.ParseQueryString(queryString).Get("prompt");
    var state = HttpUtility.ParseQueryString(queryString).Get("state");

    var parameters = new Dictionary<string, string>
      {
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "redirect_uri", "http://localhost:3000/redirect" },
        { "code", code },
        { "grant_type", "authorization_code" }
      };

    var encodedContent = new FormUrlEncodedContent(parameters);

    using var client = new HttpClient();

    var res = await client.PostAsync("https://oauth2.googleapis.com/token", encodedContent);

    var responseContent = await res.Content.ReadAsStringAsync();

    var googleAccessTokenDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

    googleAccessTokenDictionary.TryGetValue("access_token", out var googleAccessToken);
    googleAccessTokenDictionary.TryGetValue("expires_in", out var expiresIn);
    googleAccessTokenDictionary.TryGetValue("scope", out var scope);
    googleAccessTokenDictionary.TryGetValue("token_type", out var tokenType);
    googleAccessTokenDictionary.TryGetValue("id_token", out var idToken);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", googleAccessToken);

    var usertInfo = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
    var usertInfoContent = await usertInfo.Content.ReadAsStringAsync();

    var userInfoDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(usertInfoContent);

    userInfoDictionary.TryGetValue("sub", out var sub);
    userInfoDictionary.TryGetValue("name", out var name);
    userInfoDictionary.TryGetValue("given_name", out var givenName);
    userInfoDictionary.TryGetValue("family_name", out var familyName);
    userInfoDictionary.TryGetValue("picture", out var picture);
    userInfoDictionary.TryGetValue("email", out var email);
    userInfoDictionary.TryGetValue("email_verified", out var emailVerified);
    userInfoDictionary.TryGetValue("locale", out var locale);

    var googleUserResult = await googleUserRepository.GetBySub(sub);

    if(googleUserResult.IsFailure) {

      var userResult = await userRepository.GetByEmail(email);
      if(userResult.IsSuccess) {

        var user = userResult.Value;
        var newRefreshToken = Guid.NewGuid().ToString();

        var newGoogleUser = new GoogleUser {
          Id = user.Id,
          Email = email,
          GivenName = givenName,
          FamilyName = familyName,
          Sub = sub,
          Locale = locale,
          Name = name,
          Picture = picture,
          EmailVerified = bool.TryParse(emailVerified, out var isVerified) ? isVerified : false,
          CreationTime = DateTime.UtcNow,
          LastUpdateTime = DateTime.UtcNow,
        };

        await googleUserRepository.Create(newGoogleUser);

        var newSession = new ExternalAuthSession {
          RefreshToken = newRefreshToken,
          UserId = user.Id,
          CreationTime = DateTime.UtcNow,
          LastUpdateTime = DateTime.UtcNow
        };

        await sessionRepository.Create(newSession);

        var sessionResult = await sessionRepository.GetByRefreshToken(newRefreshToken);
        if(sessionResult.IsFailure) return Results.BadRequest("No session was found");
        var session = sessionResult.Value;

        response.AppendRefreshTokenCookie(newRefreshToken);

        var accessToken = authService.GenerateAccessToken(user.Id.ToString());

        var sessionData = new SessionData {
          Id = session.Id,
          UserId = session.UserId,
          UserEmail = user.Email,
          UserNickname = user.Nickname
        };

        return Results.Ok(new {
          accessToken = accessToken,
          sessionData = sessionData
        });

      } else {

        var newUser = new User {
          Email = email,
          Nickname = givenName,
          CreationTime = DateTime.UtcNow,
          LastUpdateTime = DateTime.UtcNow,
        };

        await userRepository.Create(newUser);

        var newUserResult = await userRepository.GetByEmail(newUser.Email);
        if(newUserResult.IsFailure) return Results.BadRequest("No user was found");
        var newUserFetched = newUserResult.Value;

        var newGoogleUser = new GoogleUser {
          Id = newUserFetched.Id,
          Email = email,
          GivenName = givenName,
          FamilyName = familyName,
          Sub = sub,
          Locale = locale,
          Name = name,
          Picture = picture,
          EmailVerified = bool.TryParse(emailVerified, out var isVerified) ? isVerified : false,
          CreationTime = DateTime.UtcNow,
          LastUpdateTime = DateTime.UtcNow,
        };

        await googleUserRepository.Create(newGoogleUser);

        var newRefreshToken = Guid.NewGuid().ToString();

        var newSession = new ExternalAuthSession {
          RefreshToken = newRefreshToken,
          UserId = newUser.Id,
          CreationTime = DateTime.UtcNow,
          LastUpdateTime = DateTime.UtcNow
        };

        await sessionRepository.Create(newSession);

        var sessionResult = await sessionRepository.GetByRefreshToken(newRefreshToken);
        if(sessionResult.IsFailure) return Results.BadRequest("No session was found");
        var session = sessionResult.Value;

        response.AppendRefreshTokenCookie(newRefreshToken);

        var accessToken = authService.GenerateAccessToken(newUser.Id.ToString());

        var sessionData = new SessionData {
          Id = session.Id,
          UserId = session.UserId,
          UserEmail = newUser.Email,
          UserNickname = newUser.Nickname
        };

        return Results.Ok(new {
          accessToken = accessToken,
          sessionData = sessionData
        });
      }
    } else {

      var googleUser = googleUserResult.Value;
      var updatedGoogleUser = new GoogleUser {
        Id = googleUser.Id,
        CreationTime = googleUser.CreationTime,
        LastUpdateTime = DateTime.UtcNow,
        Email = email,
        EmailVerified = bool.TryParse(emailVerified, out var isVerified) ? isVerified : false,
        FamilyName = familyName,
        GivenName = givenName,
        Locale = locale,
        Name = name,
        Picture = picture,
        Sub = sub
      };

      await googleUserRepository.Update(updatedGoogleUser);

      var userResult = await userRepository.GetById(updatedGoogleUser.Id);
      if(userResult.IsFailure) return Results.BadRequest($"User with id {updatedGoogleUser.Id} has not been found ");


      var newRefreshToken = Guid.NewGuid().ToString();

      var newSession = new ExternalAuthSession {
        RefreshToken = newRefreshToken,
        UserId = updatedGoogleUser.Id,
        CreationTime = DateTime.UtcNow,
        LastUpdateTime = DateTime.UtcNow
      };

      await sessionRepository.Create(newSession);

      var sessionResult = await sessionRepository.GetByRefreshToken(newRefreshToken);
      if(sessionResult.IsFailure) return Results.BadRequest("No session was found");
      var session = sessionResult.Value;

      response.AppendRefreshTokenCookie(newRefreshToken);

      var accessToken = authService.GenerateAccessToken(googleUser.Id.ToString());

      var sessionData = new SessionData {
        Id = session.Id,
        UserId = session.UserId,
        UserEmail = userResult.Value.Email,
        UserNickname = userResult.Value.Nickname
      };

      return Results.Ok(new {
        accessToken = accessToken,
        sessionData = sessionData
      });

    }
  }
}