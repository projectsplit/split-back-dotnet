namespace SplitBackApi.Endpoints;

public static partial class GuestEndpoints {
  
  public static void MapGuestEndpoints(this IEndpointRouteBuilder app) {
    var guestGroup = app.MapGroup("/guest")
      .WithTags("Guest");
      // .AllowAnonymous();
      
    guestGroup.MapPost("/addGuest", AddGuest);
    guestGroup.MapPost("/restoreGuest", RestoreGuest);
  }
}