using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static async Task<IResult> Callback(
    HttpContext httpContext,
    HttpRequest httpRequest
  ) {   
    
    var queryString = httpRequest.QueryString;
    var code = HttpUtility.ParseQueryString(httpContext.Request.QueryString.Value).Get("code");
    var scopee = HttpUtility.ParseQueryString(httpContext.Request.QueryString.Value).Get("scope");
    var authuser = HttpUtility.ParseQueryString(httpContext.Request.QueryString.Value).Get("authUser");
    var prompt = HttpUtility.ParseQueryString(httpContext.Request.QueryString.Value).Get("prompt");
    var state = HttpUtility.ParseQueryString(httpContext.Request.QueryString.Value).Get("state");

    var parameters = new Dictionary<string, string>
      {
        { "client_id", "363916783985-7b691nbt6mo69c2ieb0eb8njaojea91u.apps.googleusercontent.com" },
        { "client_secret", "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" },
        { "redirect_uri", "https://localhost:7014/auth/google/callback" },
        { "code", code },
        { "grant_type", "authorization_code" }
      };
      
    var encodedContent = new FormUrlEncodedContent(parameters);

    using var client = new HttpClient();

    var response = await client.PostAsync("https://oauth2.googleapis.com/token", encodedContent);

    var responseContent = await response.Content.ReadAsStringAsync();
    
    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
    
    dictionary.TryGetValue("access_token", out var accessToken);
    dictionary.TryGetValue("expires_in", out var expiresIn);
    dictionary.TryGetValue("scope", out var scope);
    dictionary.TryGetValue("token_type", out var tokenType);
    dictionary.TryGetValue("id_token", out var idToken);
    
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    
    var responseB = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
    var responseContentB = await responseB.Content.ReadAsStringAsync();
    
    var dictionaryB = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContentB);
    
    dictionaryB.TryGetValue("sub", out var sub);
    dictionaryB.TryGetValue("name", out var name);
    dictionaryB.TryGetValue("given_name", out var givenName);
    dictionaryB.TryGetValue("family_name", out var familyName);
    dictionaryB.TryGetValue("picture", out var picture);
    dictionaryB.TryGetValue("email", out var email);
    dictionaryB.TryGetValue("email_verified", out var emailVerified);
    dictionaryB.TryGetValue("locale", out var locale);
    
    return Results.Ok();
  }
}
