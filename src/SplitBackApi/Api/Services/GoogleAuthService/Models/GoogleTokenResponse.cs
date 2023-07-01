using Newtonsoft.Json;

namespace SplitBackApi.Api.Services.GoogleAuthService.Models;

public class GoogleTokenResponse
{
  [JsonProperty("access_token")]
  public string AccessToken { get; set; }
}