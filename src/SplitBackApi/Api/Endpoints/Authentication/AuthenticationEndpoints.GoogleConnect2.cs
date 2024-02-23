using System.Web;
using Microsoft.Extensions.Options;
using SplitBackApi.Api.Endpoints.Authentication.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Api.Services;
using SplitBackApi.Configuration;
using SplitBackApi.Data.Repositories.GoogleUserRepository;
using SplitBackApi.Data.Repositories.SessionRepository;
using SplitBackApi.Data.Repositories.UserRepository;
using SplitBackApi.Domain.Models;
using CSharpFunctionalExtensions;
using MongoDB.Bson;
using SplitBackApi.Api.Services.GoogleAuthService;
using SplitBackApi.Api.Services.GoogleAuthService.Models;
using SplitBackApi.Api.Models;

namespace SplitBackApi.Api.Endpoints.Authentication;

public static partial class AuthenticationEndpoints
{
  public static async Task<Microsoft.AspNetCore.Http.IResult> GoogleConnect2(
    ContinueWithGoogleRequest2 request,
    HttpResponse response,
    IOptions<AppSettings> appSettings,
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ISessionRepository sessionRepository,
    AuthService authService,
    GoogleAuthService googleAuthService)
  {

    var googleUserInfo = request;

    var userDocumentsResult = await UpdateOrCreateUserDocuments2(userRepository, googleUserRepository, googleUserInfo);
    if (userDocumentsResult.IsFailure) Results.BadRequest(userDocumentsResult.Error);
    var user = userDocumentsResult.Value;

    var newExternalSession = CreateNewExternalSession2(user.Id);
    await sessionRepository.Create(newExternalSession);

    response.AppendRefreshTokenCookie(newExternalSession.RefreshToken);
    var accessToken = authService.GenerateAccessToken(user.Id.ToString());
    var sessionData = CreateSessionData(user, newExternalSession);

    return Results.Ok(new SignInResponse(accessToken, sessionData));
  }

  private static async Task<Result<User>> UpdateOrCreateUserDocuments2(
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    GoogleUserInfo googleUserInfo)
  {
    var (googleUserExists, googleUser) = await googleUserRepository.GetBySub(googleUserInfo.Sub);

    if (googleUserExists)
    {
      var userResult = await userRepository.GetById(googleUser.Id);
      if (userResult.IsFailure) return Result.Failure<User>($"User with id {googleUser.Id} has not been found ");
      var existingUser = userResult.Value;

      var updatedGoogleUser = CreateUpdatedGoogleUser2(googleUserInfo, googleUser);
      await googleUserRepository.Update(updatedGoogleUser);

      return existingUser;
    }

    var user = await GetOrCreateUser2(userRepository, googleUserInfo);

    var newGoogleUser = CreateNewGoogleUser(googleUserInfo, user);
    await googleUserRepository.Create(newGoogleUser);

    return user;
  }

  private static async Task<User> GetOrCreateUser2(
    IUserRepository userRepository,
    GoogleUserInfo googleUserInfo)
  {
    var userResult = await userRepository.GetByEmail(googleUserInfo.Email);

    if (userResult.IsSuccess)
    {
      return userResult.Value;
    }

    var newUser = CreateNewUser2(googleUserInfo);
    await userRepository.Create(newUser);

    return newUser;
  }

  private static SessionData CreateSessionData2(User newUser, ExternalAuthSession externalSession)
  {
    return new SessionData
    {
      Id = externalSession.Id,
      UserId = externalSession.UserId,
      UserEmail = newUser.Email,
      UserNickname = newUser.Nickname
    };
  }

  private static User CreateNewUser2(GoogleUserInfo googleUserInfo)
  {
    return new User
    {
      Id = ObjectId.GenerateNewId().ToString(),
      Email = googleUserInfo.Email,
      Nickname = googleUserInfo.GivenName,
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
    };
  }

  private static GoogleUser CreateNewGoogleUser2(GoogleUserInfo googleUserInfo, User user)
  {
    return new GoogleUser
    {
      Id = user.Id,
      Email = googleUserInfo.Email,
      GivenName = googleUserInfo.GivenName,
      FamilyName = googleUserInfo.FamilyName,
      Sub = googleUserInfo.Sub,
      Locale = googleUserInfo.Locale,
      Name = googleUserInfo.Name,
      Picture = googleUserInfo.Picture,
      EmailVerified = googleUserInfo.EmailVerified,
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow,
    };
  }

  private static GoogleUser CreateUpdatedGoogleUser2(GoogleUserInfo googleUserInfo, GoogleUser googleUser)
  {
    return new GoogleUser
    {
      Id = googleUser.Id,
      CreationTime = googleUser.CreationTime,
      LastUpdateTime = DateTime.UtcNow,
      Email = googleUserInfo.Email,
      EmailVerified = googleUserInfo.EmailVerified,
      FamilyName = googleUserInfo.FamilyName,
      GivenName = googleUserInfo.GivenName,
      Locale = googleUserInfo.Locale,
      Name = googleUserInfo.Name,
      Picture = googleUserInfo.Picture,
      Sub = googleUserInfo.Sub
    };
  }

  private static ExternalAuthSession CreateNewExternalSession2(string userId)
  {
    var newRefreshToken = Guid.NewGuid().ToString();

    return new ExternalAuthSession
    {
      Id = ObjectId.GenerateNewId().ToString(),
      RefreshToken = newRefreshToken,
      UserId = userId,
      CreationTime = DateTime.UtcNow,
      LastUpdateTime = DateTime.UtcNow
    };
  }
}