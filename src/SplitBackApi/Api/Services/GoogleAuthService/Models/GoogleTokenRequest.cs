namespace SplitBackApi.Api.Services.GoogleAuthService.Models;

public class GoogleTokenRequest
{
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
  public string RedirectUri { get; set; }
  public string Code { get; set; }
  public string GrantType { get; set; }

  public Dictionary<string, string> ToDictionary()
  {
    var dict = new Dictionary<string, string>
      {
        {"client_id", ClientId},
        {"client_secret", ClientSecret},
        {"redirect_uri", RedirectUri},
        {"code", Code},
        {"grant_type", GrantType}
      };
    return dict;
  }
}