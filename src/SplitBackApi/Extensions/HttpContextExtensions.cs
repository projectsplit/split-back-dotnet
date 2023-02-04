using CSharpFunctionalExtensions;
using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class HttpContextExtensions {

  public static Result<ObjectId> GetAuthorizedUserId(this HttpContext httpContext) {

    var userClaim = httpContext.User.FindFirst("userId");
    var userID = new ObjectId(userClaim?.Value);

    if(userClaim is null) return Result.Failure<ObjectId>("User claim is null");

    return userID;
  }
}