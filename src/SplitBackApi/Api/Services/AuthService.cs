using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SplitBackApi.Configuration;

namespace SplitBackApi.Api.Services;

public class AuthService {

  private readonly JwtSettings _jwtSettings;
  private readonly string _frontEndUrl;

  public AuthService(IOptions<AppSettings> appSettings) {

    _jwtSettings = appSettings.Value.Jwt;
    _frontEndUrl = appSettings.Value.FrontendUrl;
  }

  public string GenerateAccessToken(string userId) {

    var expires = DateTime.Now.AddMinutes(20);

    var subject = new ClaimsIdentity(new[] {
      new Claim("userId", userId)
    });

    var token = CreateJwtToken(subject, expires);

    return token;
  }
  
  public string GenerateEmailLink(string unique, string email, bool isNewUser){

    var expires = DateTime.Now.AddMinutes(2);

    var subject = new ClaimsIdentity(new[] {
      new Claim("new-user", isNewUser.ToString()),
      new Claim("email", email),
      new Claim("unique", unique),
    });

    var token = CreateJwtToken(subject, expires);

    return $"{_frontEndUrl}/v/{token}";
  }

  public string GenerateSignInRequestToken(string unique, string email) {

    var expires = DateTime.Now.AddMinutes(2);

    var subject = new ClaimsIdentity(new[] {
      new Claim("type", "sign-in"),
      new Claim("email", email),
      new Claim("unique", unique),
    });

    var token = CreateJwtToken(subject, expires);

    return token;
  }

  public string GenerateSignUpRequestToken(string unique, string email, string nickname) {

    var expires = DateTime.Now.AddMinutes(2);

    var subject = new ClaimsIdentity(new[] {
      new Claim("type", "sign-up"),
      new Claim("email", email),
      new Claim("nickname", nickname),
      new Claim("unique", unique),
    });

    var token = CreateJwtToken(subject, expires);

    return token;
  }

  public Result<JwtSecurityToken> VerifyToken(string token) {

    var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

    try {
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken(token, new TokenValidationParameters {
        ValidateIssuerSigningKey = false,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        // Clockskew Zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      return (JwtSecurityToken)validatedToken;

    } catch(Exception ex) {
      
      return Result.Failure<JwtSecurityToken>(ex.Message);
    }
  }

  private string CreateJwtToken(ClaimsIdentity subject, DateTime expires) {

    var secureKey = Encoding.UTF8.GetBytes(_jwtSettings.Key);
    var securityKey = new SymmetricSecurityKey(secureKey);
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
    var jwtTokenHandler = new JwtSecurityTokenHandler();

    var tokenDescriptor = new SecurityTokenDescriptor {
      Subject = subject,
      Expires = expires,
      Issuer = _jwtSettings.Issuer,
      Audience = _jwtSettings.Audience,
      SigningCredentials = credentials
    };

    return jwtTokenHandler.CreateEncodedJwt(tokenDescriptor);
  }
}
