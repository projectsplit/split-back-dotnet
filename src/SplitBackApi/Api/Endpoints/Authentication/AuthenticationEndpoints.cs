namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints {
  
  public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app) {

    var authGroup = app.MapGroup("/auth")
      .WithTags("Authentication")
      .AllowAnonymous();

    authGroup.MapPost("/verify-token", VerifyToken);
    authGroup.MapPost("/sign-in", SignIn);
    authGroup.MapPost("/request-sign-up", RequestSignUp);
    authGroup.MapPost("/request-sign-in", RequestSignIn);
    authGroup.MapPost("/refresh-token", RefreshToken);
  }
}
