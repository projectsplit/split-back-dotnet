using SplitBackApi.Domain;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransferEndpoints {

  public static void MapTransferEndpoints(this IEndpointRouteBuilder app) {

    var transferGroup = app.MapGroup("/transfer")
      .WithTags("Transfers");

    transferGroup.MapPost("/create", CreateTransfer);
    transferGroup.MapPost("/edit", EditTransfer);
    transferGroup.MapPost("/delete", DeleteTransfer);
  }
}