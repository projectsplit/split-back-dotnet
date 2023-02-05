using CSharpFunctionalExtensions;

namespace SplitBackApi.Extensions;

public static class HttpContextExtensions {

  public static Result<string> GetAuthorizedUserId(this HttpContext httpContext) {

    var userClaim = httpContext.User.FindFirst("userId");
    if(userClaim is null) return Result.Failure<string>("Claim is null");

    var userId = userClaim.Value;
    if(userId is null) return Result.Failure<string>("Claim value is null");

    return userId;
  }
}