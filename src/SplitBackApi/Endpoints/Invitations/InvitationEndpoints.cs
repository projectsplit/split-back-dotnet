namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {
  
  public static void MapInvitationEndpoints(this IEndpointRouteBuilder app) {
    var invitationGroup = app.MapGroup("/invitation")
      .WithTags("Invitations");
      // .AllowAnonymous();
      
    invitationGroup.MapPost("/create", Create);
    invitationGroup.MapPost("/regenerate", Regenerate);
    invitationGroup.MapPost("/verify", Verify);
    invitationGroup.MapPost("/accept", Accept);
  }
}