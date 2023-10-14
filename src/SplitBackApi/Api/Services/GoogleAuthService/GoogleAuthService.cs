using System.Web;
using Microsoft.Extensions.Options;
using SplitBackApi.Api.Services.GoogleAuthService.Models;
using SplitBackApi.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using CSharpFunctionalExtensions;
using System.Net;

namespace SplitBackApi.Api.Services.GoogleAuthService;

public class GoogleAuthService
{
  private readonly string _clientId;
  private readonly string _clientSecret;
  private readonly HttpClient _httpClient;

  public GoogleAuthService(
    IOptions<AppSettings> appSettings,
    HttpClient httpClient)
  {
    _clientId = appSettings.Value.Google.ClientId;
    _clientSecret = appSettings.Value.Google.ClientSecret;
    _httpClient = httpClient;
  }

  public string GenerateSignInWithGoogleUrl()
  {
    const string Scope = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
    const string AccessType = "offline";
    const string IncludeGrantedScopes = "true";
    const string ResponseType = "code";
    const string State = "state_parameter_passthrough_value";
    const string RedirectUri = "http://localhost:3000/redirect";
    const string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";

    var queryParams = HttpUtility.ParseQueryString(string.Empty);
    queryParams["scope"] = Scope;
    queryParams["access_type"] = AccessType;
    queryParams["include_granted_scopes"] = IncludeGrantedScopes;
    queryParams["response_type"] = ResponseType;
    queryParams["state"] = State;
    queryParams["redirect_uri"] = RedirectUri;
    queryParams["client_id"] = _clientId;

    var googleAuthUrl = new UriBuilder(GoogleAuthUrl)
    {
      Query = queryParams.ToString()
    }.Uri;

    return googleAuthUrl.ToString();
  }

  public async Task<Result<GoogleUserInfo>> GetGoogleUserInfo(string code)
  {
    var parameters = new GoogleTokenRequest
    {
      ClientId = _clientId,
      ClientSecret = _clientSecret,
      RedirectUri = "http://localhost:3000/redirect",
      Code = code,
      GrantType = "authorization_code"
    };

    var encodedContent = new FormUrlEncodedContent(parameters.ToDictionary());

    var googleAcceessTokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", encodedContent);
    
    if (googleAcceessTokenResponse.StatusCode is not HttpStatusCode.OK){
      return Result.Failure<GoogleUserInfo>("Failed to get google access token");
    }

    var responseContent = await googleAcceessTokenResponse.Content.ReadAsStringAsync();

    var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);

    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

    var googleUserInfoResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
    
    if (googleAcceessTokenResponse.StatusCode is not HttpStatusCode.OK){
      return Result.Failure<GoogleUserInfo>("Failed to get google user info");
    }
    
    var googleUserInfoContent = await googleUserInfoResponse.Content.ReadAsStringAsync();

    var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(googleUserInfoContent);

    return userInfo;
  }
}