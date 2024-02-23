namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {
  
  public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app) {

    var authGroup = app.MapGroup("/auth")
      .WithTags("Authentication")
      .AllowAnonymous();

    authGroup.MapPost("/refresh-token", RefreshToken);
    
    authGroup.MapPost("/email/send-link", EmailSendLink);
    authGroup.MapGet("/email/verify-link-token", EmailVerifyLinkToken);
    authGroup.MapGet("/email/connect", EmailConnect);
    
    authGroup.MapPost("/google/connect", GoogleConnect);
    authGroup.MapPost("/google/connect2", GoogleConnect2);
    authGroup.MapGet("/google/url", GoogleUrl);
  }
}
