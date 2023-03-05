namespace SplitBackApi.Api.Endpoints.Groups;

public static partial class GuestEndpoints {

  public static void MapGuestEndpoints(this IEndpointRouteBuilder app) {

    var guestGroup = app.MapGroup("/guest")
      .WithTags("Guests");

    guestGroup.MapPost("/createguest", CreateGuest);
    guestGroup.MapPost("/removeguest", RemoveGuest);

  }
}