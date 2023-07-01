using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using SplitBackApi.Configuration;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {

  public static IResult GetGoogleUrl(
   IOptions<AppSettings> appSettings
  ) {

    var clientId = appSettings.Value.Google.ClientId;

    var urlBuilder = new StringBuilder("https://accounts.google.com/o/oauth2/v2/auth?");
    urlBuilder.Append($"scope={HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile")}&");
    urlBuilder.Append("access_type=offline&");
    urlBuilder.Append("include_granted_scopes=true&");
    urlBuilder.Append("response_type=code&");
    urlBuilder.Append("state=state_parameter_passthrough_value&");
    urlBuilder.Append($"redirect_uri={HttpUtility.UrlEncode("http://localhost:3000/redirect")}&");
    urlBuilder.Append($"client_id={HttpUtility.UrlEncode(clientId)}");

    var redirectUrl = urlBuilder.ToString();
    return Results.Ok(redirectUrl);

  }
}
