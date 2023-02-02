using MongoDB.Bson;

namespace SplitBackApi.Extensions;

public static class HttpContextExtensions {

  public static ObjectId GetAuthorizedUserId(this HttpContext httpContext) {

    var userClaim = httpContext.User.FindFirst("userId");
    var userID = new ObjectId(userClaim?.Value);

    if(userClaim is null) throw new Exception();

    return userID;
  }

  public static async Task<string> SerializationAsync(this HttpContext httpContext) {

    var buffer = new MemoryStream();
    await httpContext.Request.Body.CopyToAsync(buffer);
    buffer.Seek(0, SeekOrigin.Begin);

    var serializedRequest = await new StreamReader(buffer).ReadToEndAsync();

    buffer.Seek(0, SeekOrigin.Begin);
    httpContext.Request.Body = buffer;

    return serializedRequest;
  }
}