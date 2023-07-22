using System.Security.Claims;
using SplitBackApi.Data.Repositories.SessionRepository;


// https://stackoverflow.com/questions/50530363/aggregate-lookup-with-c-sharp

namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GroupEndpoints {

  private static async Task<Microsoft.AspNetCore.Http.IResult> GetLatestSessions(
    ClaimsPrincipal claimsPrincipal,
    ISessionRepository sessionRepository,
    HttpRequest request
  ) {
    var limitQuery = request.Query["limit"].ToString();
    if (string.IsNullOrEmpty(limitQuery)) return Results.BadRequest("limit query is missing");
    if (int.TryParse(limitQuery, out var limit) is false) return Results.BadRequest("invalid limit");
    var lastQuery = request.Query["last"].ToString();
    if (string.IsNullOrEmpty(lastQuery)) return Results.BadRequest("last query is missing");
    if (DateTime.TryParse(lastQuery, out var last) is false) return Results.BadRequest("invalid last");

    var sessions = await sessionRepository.GetLatest(limit, last);

    return Results.Ok(sessions);
  }
}