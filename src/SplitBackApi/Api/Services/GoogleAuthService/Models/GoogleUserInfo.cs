using Newtonsoft.Json;

namespace SplitBackApi.Api.Services.GoogleAuthService.Models;

public class GoogleUserInfo
{
  [JsonProperty("sub")]
  public string Sub { get; set; }

  [JsonProperty("name")]
  public string Name { get; set; }

  [JsonProperty("given_name")]
  public string GivenName { get; set; }

  [JsonProperty("family_name")]
  public string FamilyName { get; set; }

  [JsonProperty("picture")]
  public string Picture { get; set; }

  [JsonProperty("email")]
  public string Email { get; set; }

  [JsonProperty("email_verified")]
  public bool EmailVerified { get; set; }

  [JsonProperty("locale")]
  public string Locale { get; set; }
}