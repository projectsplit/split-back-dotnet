using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GoogleUserRepository;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Api.Services.GoogleAuthService;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints
{
  public static async Task<Microsoft.AspNetCore.Http.IResult> GoogleOneTapConnect(
    ContinueWithOneTapGoogleRequest request,
    HttpResponse response,
    IOptions<AppSettings> appSettings,
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ISessionRepository sessionRepository,
    AuthService authService,
    GoogleAuthService googleAuthService)
  {
    // var queryString = request.RedirectUrlSearchParameters;
    // var code = HttpUtility.ParseQueryString(queryString).Get("code");

    // var googleUserInfoResult = await googleAuthService.GetGoogleUserInfo(code);
    // if (googleUserInfoResult.IsFailure) return Results.BadRequest(googleUserInfoResult.Error);
    // var googleUserInfo = googleUserInfoResult.Value;

    // var userDocumentsResult = await UpdateOrCreateUserDocuments(userRepository, googleUserRepository, googleUserInfo);
    // if (userDocumentsResult.IsFailure) Results.BadRequest(userDocumentsResult.Error);
    // var user = userDocumentsResult.Value;

    // var newExternalSession = CreateNewExternalSession(user.Id);
    // await sessionRepository.Create(newExternalSession);

    // response.AppendRefreshTokenCookie(newExternalSession.RefreshToken);
    // var accessToken = authService.GenerateAccessToken(user.Id.ToString());
    // var sessionData = CreateSessionData(user, newExternalSession);

    // return Results.Ok(new SignInResponse(accessToken, sessionData));
    return Results.Ok();
  }
}