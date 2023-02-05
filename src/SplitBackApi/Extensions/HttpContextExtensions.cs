using CSharpFunctionalExtensions;
using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class HttpContextExtensions {

  public static Result<string> GetAuthorizedUserId(this HttpContext httpContext) {

    var userClaim = httpContext.User.FindFirst("userId");
    var userID = userClaim?.Value;

    if(userClaim is null) return Result.Failure<string>("User claim is null");

    return userID;
  }
}