using System.Security.Claims;
using SplitBackApi.Api.Endpoints.Transactions.Requests;
using SplitBackApi.Api.Extensions;
using SplitBackApi.Data.Repositories.GroupRepository;
using SplitBackApi.Domain.Extensions;
using SplitBackApi.Domain.Services;

namespace SplitBackApi.Api.Endpoints.Transactions;

public static partial class TransactionEndpoints {

  private static async Task<IResult> Pending(
    ClaimsPrincipal claimsPrincipal,
    TransactionService transactionService,
    IGroupRepository groupRepository,
    PendingTransactionsRequest request
  ) {

    var authenticatedUserId = claimsPrincipal.GetAuthenticatedUserId();
    
    var groupResult = await groupRepository.GetById(request.GroupId);
    if(groupResult.IsFailure) return Results.BadRequest(groupResult.Error);
    var group = groupResult.Value;
    
    var member = group.GetMemberByUserId(authenticatedUserId);
    if(member is null) return Results.BadRequest($"User with id {authenticatedUserId} is not a member of group with id {request.GroupId}"); //not required? idk
    
    var pendingResult = await transactionService.PendingTransactionsAsync(group.Id);
    if(pendingResult.IsFailure) return Results.BadRequest(pendingResult.Error);
    
    return Results.Ok(pendingResult.Value);
  }
}