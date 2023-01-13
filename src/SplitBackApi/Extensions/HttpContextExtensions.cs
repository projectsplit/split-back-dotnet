using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class HttpContextExtensions
{

  public static ObjectId GetAuthorizedUserId(this HttpContext httpContext)
  {

    var userClaim = httpContext.User.FindFirst("userId");
    var userID = new ObjectId(userClaim?.Value);

    if (userClaim is null) throw new Exception();

    return userID;
  }
}