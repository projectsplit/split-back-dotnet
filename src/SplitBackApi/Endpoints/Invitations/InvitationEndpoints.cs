namespace SplitBackApi.Endpoints;

public static partial class InvitationEndpoints {
  
  public static void MapInvitationEndpoints(this IEndpointRouteBuilder app) {
    var invitationGroup = app.MapGroup("/invitation")
      .WithTags("Invitations");
      
    invitationGroup.MapPost("/create-basic", CreateBasic);
    invitationGroup.MapPost("/create-replacement", CreateReplacement);
    invitationGroup.MapPost("/verify", Verify);
    invitationGroup.MapPost("/accept", Accept);
  }
}