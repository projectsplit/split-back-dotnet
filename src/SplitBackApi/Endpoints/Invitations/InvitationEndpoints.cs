namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {
  
  public static void MapInvitationEndpoints(this IEndpointRouteBuilder app) {
    var invitationGroup = app.MapGroup("/invitation")
      .WithTags("Invitations");
      // .AllowAnonymous();
      
    invitationGroup.MapPost("/createUserInvitation", CreateUserInvitation);
    invitationGroup.MapPost("/createGuestInvitation", CreateGuestInvitation);
    invitationGroup.MapPost("/regenerateUserInvitation", RegenerateUserInvitation);
    invitationGroup.MapPost("/regenerateGuestInvitation", RegenerateGuestInvitation);
    invitationGroup.MapPost("/verify", Verify);
    invitationGroup.MapPost("/accept", Accept);
  }
}