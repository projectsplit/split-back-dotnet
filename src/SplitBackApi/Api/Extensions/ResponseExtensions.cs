namespace SplitBackApi.Api.Extensions;

public static class ResponseExtensions {

  public static void AppendRefreshTokenCookie(this HttpResponse response, string refreshToken) {

    response.Cookies.Append("refresh-token", refreshToken, AuthCookieOptions("/auth/refresh-token"));
  }
  
  public static void AppendUniqueCookie(this HttpResponse response, string unique) {

    response.Cookies.Append("unique", unique, AuthCookieOptions("/auth/sign-in"));
  }

  public static void DeleteUniqueCookie(this HttpResponse response) {

    response.Cookies.Delete("unique", AuthCookieOptions("/auth/sign-in"));
  }

  private static CookieOptions AuthCookieOptions(string path) {

    return new CookieOptions {
      SameSite = SameSiteMode.Lax,
      HttpOnly = true,
      Path = path,
      Expires = DateTime.UtcNow.AddDays(30),
      MaxAge = TimeSpan.FromDays(30)
    };
  }
}
