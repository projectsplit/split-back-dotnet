namespace SplitBackApi.Endpoints;

public static partial class GuestInvitationEndpoints {

  public static void MapGuestInvitationEndpoints(this IEndpointRouteBuilder app) {
    var guestInvitationGroup = app.MapGroup("/guestInvitation")
      .WithTags("GuestInvitations");
    // .AllowAnonymous();

    guestInvitationGroup.MapPost("/create", Create);
    guestInvitationGroup.MapPost("/regenerate", Regenerate);
    guestInvitationGroup.MapPost("/verify", Verify);
    guestInvitationGroup.MapPost("/accept", Accept);
  }
}