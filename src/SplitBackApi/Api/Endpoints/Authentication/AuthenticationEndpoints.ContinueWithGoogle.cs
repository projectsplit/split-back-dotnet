using System.Net.Http.Headers;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> ContinueWithGoogle(
    HttpResponse response,
    HttpRequest request,
    IOptions<AppSettings> appSettings,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    AuthService authService
  ) {

    var clientId = appSettings.Value.Google.ClientId;
    var clientSecret = appSettings.Value.Google.ClientSecret;

    var queryString = request.QueryString;
    var code = HttpUtility.ParseQueryString(request.QueryString.Value).Get("code");
    var scopee = HttpUtility.ParseQueryString(request.QueryString.Value).Get("scope");
    var authuser = HttpUtility.ParseQueryString(request.QueryString.Value).Get("authUser");
    var prompt = HttpUtility.ParseQueryString(request.QueryString.Value).Get("prompt");
    var state = HttpUtility.ParseQueryString(request.QueryString.Value).Get("state");

    var parameters = new Dictionary<string, string>
      {
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "redirect_uri", "https://localhost:7014/auth/google/callback" },
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


    var userResult = await userRepository.GetByEmail(email);

    if(userResult.IsSuccess) {
      var user = userResult.Value;
      var newRefreshToken = Guid.NewGuid().ToString();

      var newSession = new Session {
        RefreshToken = newRefreshToken,
        UserId = user.Id,
        Unique = "google",
        CreationTime = DateTime.UtcNow,
        LastUpdateTime = DateTime.UtcNow
      };

      await sessionRepository.Create(newSession);
      response.AppendRefreshTokenCookie(newRefreshToken);

      var accessToken = authService.GenerateAccessToken(user.Id.ToString());

      return Results.Ok(accessToken);

    } else {

      var newUser = new User {
        Email = email,
        Nickname = givenName,
        CreationTime = DateTime.UtcNow,
        LastUpdateTime = DateTime.UtcNow,
      };

      await userRepository.Create(newUser);
      var newRefreshToken = Guid.NewGuid().ToString();

      var newSession = new Session {
        RefreshToken = newRefreshToken,
        UserId = newUser.Id,
        Unique = "google",
        CreationTime = DateTime.UtcNow,
        LastUpdateTime = DateTime.UtcNow
      };

      await sessionRepository.Create(newSession);
      response.AppendRefreshTokenCookie(newRefreshToken);

      var accessToken = authService.GenerateAccessToken(newUser.Id.ToString());

      return Results.Ok(accessToken);
    }
  }
}


    // Get authorization_code from request
    
    // With this request access token from google
    
    // Get user info with access token
    
    // Get email from info
    
    // if user with email not exists
    
        // Create User with email
        // Create a Session for new User
        // Return refresh & access token
        
    // if user already exists
    
        // Create a Session for new User
        // Return refresh & access token