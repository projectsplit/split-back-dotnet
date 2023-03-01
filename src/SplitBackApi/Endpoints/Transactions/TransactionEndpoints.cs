using SplitBackApi.Domain;
using SplitBackApi.Extensions;

namespace SplitBackApi.Endpoints;

public static partial class TransactionEndpoints {
  
  public static void MapTransactionEndpoints(this IEndpointRouteBuilder app) {
    
    var expenseGroup = app.MapGroup("/transaction")
      .WithTags("Transactions");
     
    expenseGroup.MapPost("/pending", Pending);
    expenseGroup.MapPost("/history", History);
  }
}