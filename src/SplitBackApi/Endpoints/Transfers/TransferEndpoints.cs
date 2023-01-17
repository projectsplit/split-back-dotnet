namespace SplitBackApi.Endpoints;
public static partial class TransferEndpoints {
  public static void MapTransferEndpoints(this IEndpointRouteBuilder app) {
    var transferGroup = app.MapGroup("/transfer")
      .WithTags("Transfers")
      .AllowAnonymous();
    transferGroup.MapPost("/addTransfer", AddTransfer);
    transferGroup.MapPost("/editTransfer", EditTransfer);
    transferGroup.MapPost("/removeTransfer", RemoveTransfer);
    transferGroup.MapPost("/restoreTransfer", RestoreTransfer);
    
  }
}